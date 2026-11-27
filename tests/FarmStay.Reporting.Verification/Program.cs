using FarmStay.API.DependencyInjection;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Reporting;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Text;

var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("FarmStay/appsettings.json"), true)
 .AddJsonFile(Path.GetFullPath("FarmStay/appsettings.Development.json"), true)
 .AddUserSecrets("78bc642e-7ece-4aad-b741-ab7b10bcb3f8").AddEnvironmentVariables().Build();
var connection = config.GetConnectionString("DefaultConnection")!;
if(args.Contains("--inspect") || args.Contains("--apply")) {
 await using var live = Db(connection);
 foreach(var row in await live.Database.SqlQueryRaw<string>("SELECT CONCAT(ModuleId,' | ',ModuleName,' | ',Route,' | active=',IsActive) Value FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports')").ToListAsync()) Console.WriteLine(row);
 if(args.Contains("--apply")) { await live.Database.MigrateAsync(); Console.WriteLine("Applied reporting migration."); }
 foreach(var row in await live.Database.SqlQueryRaw<string>("SELECT CONCAT('Module=',m.ModuleName,'; role assignments=',(SELECT COUNT(*) FROM dbo.RoleModules r WHERE r.ModuleId=m.ModuleId),'; farmhouse assignments=',(SELECT COUNT(*) FROM dbo.FarmHouseModules f WHERE f.ModuleId=m.ModuleId)) Value FROM dbo.Modules m WHERE ModuleName IN ('Reporting','Reports')").ToListAsync()) Console.WriteLine(row);
 return;
}
int passed=0,failed=0;
async Task Test(string name,Func<Task> action){try{await action();passed++;Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
void Check(bool value,string message="Assertion failed"){if(!value)throw new Exception(message);}
async Task Denied(Func<Task> action){try{await action();throw new Exception("Expected rejection");}catch(TenantAccessException){}catch(ReportException e) when(e.Error==ReportError.Forbidden) {}}
var builder=new SqlConnectionStringBuilder(connection){InitialCatalog="FSReporting_Phase2_"+Guid.NewGuid().ToString("N")};
Console.WriteLine("Disposable database: "+builder.InitialCatalog);
await using var db=Db(builder.ConnectionString);
var migrator=db.GetService<IMigrator>();
const string previous="20261124190200_HardenAccountsIntegrity";
const string current="20261125000100_ConsolidateReportingIdentity";
await Test("Clean database migrates and rolls back without assignments",async()=>{await migrator.MigrateAsync();await migrator.MigrateAsync(previous);});
var farms=new[]{Farm("A","9000000101"),Farm("B","9000000102")};db.AddRange(farms);
var role=new Role{RoleName="Admin"};db.Add(role);await db.SaveChangesAsync();
var modules=await db.Modules.Where(m=>m.ModuleName=="Reporting"||m.ModuleName=="Reports").ToListAsync();
foreach(var m in modules){db.Add(new RoleModule{RoleId=role.RoleId,ModuleId=m.ModuleId});foreach(var f in farms)db.Add(new FarmHouseModule{FarmHouseId=f.FarmHouseId,ModuleId=m.ModuleId});}
await db.SaveChangesAsync();
var permissions=await db.Permissions.Where(p=>p.PermissionName.EndsWith("Report.View")||p.PermissionName.EndsWith("Report.Export")).ToListAsync();
foreach(var rm in await db.RoleModules.Where(r=>r.RoleId==role.RoleId).ToListAsync())foreach(var p in permissions)db.Add(new RoleModulePermission{RoleModuleId=rm.RoleModuleId,PermissionId=p.PermissionId,IsAllowed=modules.Single(m=>m.ModuleId==rm.ModuleId).ModuleName=="Reports"});
await db.SaveChangesAsync();db.ChangeTracker.Clear();
var before=await Snapshot();
await Test("Consolidation keeps one active Reporting identity",async()=>{await migrator.MigrateAsync(current);Check(await db.Modules.CountAsync(m=>m.IsActive&&(m.ModuleName=="Reporting"||m.ModuleName=="Reports"))==1);Check(await db.Modules.AnyAsync(m=>m.ModuleName=="Reporting"&&m.Route=="/reporting"&&m.IsActive));});
await Test("Migration rerun is idempotent",async()=>{var a=await Snapshot();await migrator.MigrateAsync(current);Check(a==await Snapshot());});
await Test("Rollback restores exact original assignment rows",async()=>{await migrator.MigrateAsync(previous);Check(before==await Snapshot());await migrator.MigrateAsync(current);});
await Test("Rollback refuses to overwrite later metadata edits",async()=>{await using var tx=await db.Database.BeginTransactionAsync();try{await db.Database.ExecuteSqlRawAsync("UPDATE dbo.Modules SET DisplayName='New decision' WHERE ModuleName='Reporting'");try{await db.Database.ExecuteSqlRawAsync(Resource("Down"));throw new Exception("Expected guarded rollback");}catch(SqlException e){Check(e.Number==51021);}}finally{await tx.RollbackAsync();}});
await Test("Unsafe merge that would gain tenant permissions is rolled back", async () => {
 await migrator.MigrateAsync(previous);
 await using(var tx=await db.Database.BeginTransactionAsync()) {
  try {
   await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE f SET IsEnabled=0 FROM dbo.FarmHouseModules f JOIN dbo.Modules m ON m.ModuleId=f.ModuleId WHERE (m.ModuleName='Reports' AND f.FarmHouseId={farms[0].FarmHouseId}) OR (m.ModuleName='Reporting' AND f.FarmHouseId={farms[1].FarmHouseId})");
   try { await db.Database.ExecuteSqlRawAsync(Resource("Up")); throw new Exception("Expected unsafe merge rejection"); }
   catch(SqlException e){Check(e.Number==51020);}
  } finally {await tx.RollbackAsync();}
 }
 Check(before==await Snapshot());await migrator.MigrateAsync(current);
});
// Restore the current schema after the historical migration/rollback checks.
await migrator.MigrateAsync();
var user=new User{FarmHouseId=farms[0].FarmHouseId,FullName="Report test",MobileNumber="9000000103",PasswordHash="test-no-login"};db.Add(user);await db.SaveChangesAsync();
var membership=new UserMembership{FarmHouseId=farms[0].FarmHouseId,UserId=user.UserId,RoleId=role.RoleId};db.Add(membership);await db.SaveChangesAsync();
var ctx=new DefaultHttpContext();ctx.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),new Claim("UserMembershipId",membership.UserMembershipId.ToString()),new Claim("FarmHouseId",farms[0].FarmHouseId.ToString()),new Claim(ClaimTypes.Role,"Admin")},"verification"));
var accessor=new FixedHttpContext{HttpContext=ctx};
var testConfig=new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",builder.ConnectionString}}).Build();
var services=new ServiceCollection();services.AddSingleton<IConfiguration>(testConfig);services.AddLogging();services.AddSingleton<IHttpContextAccessor>(accessor);services.AddInfrastructureServices(testConfig);services.AddApplicationServices();services.AddFluentValidationServices();
await using var provider=services.BuildServiceProvider();
await using var scope=provider.CreateAsyncScope();
var reporting=scope.ServiceProvider.GetRequiredService<IReportingService>();
await Test("Report grants survive migration on exact authenticated membership",async()=>{var catalog=await reporting.CatalogAsync(default);Check(catalog.Count==4&&catalog.All(r=>r.CanExport));});
foreach(var f in farms){var b=new Booking{FarmHouseId=f.FarmHouseId,CustomerUserId=user.UserId,BookingReference="PHASE2-"+f.FarmHouseId,FullName="Customer "+f.FarmHouseName,MobileNumber=user.MobileNumber,AdultCount=1,PersonCount=1,CheckInDate=DateTime.Today,CheckOutDate=DateTime.Today.AddDays(1),StayDays=1,TotalAmount=1000,BaseAmount=1000,Status="Confirmed",CreatedBy=user.UserId,CurrencyCode="INR"};db.Add(b);await db.SaveChangesAsync();var payment=new PaymentTransaction{FarmHouseId=f.FarmHouseId,BookingId=b.BookingId,PaymentType="Advance",PaymentMethod="Cash",Amount=f==farms[0]?300:900,PaymentStatus="Success",IdempotencyKey="phase2-"+f.FarmHouseId,PaidAt=DateTime.Now};db.Add(payment);await db.SaveChangesAsync();db.Add(new BookingReceipt{FarmHouseId=f.FarmHouseId,BookingId=b.BookingId,PaymentTransactionId=payment.PaymentTransactionId,ReceiptNumber="PH2-R-"+f.FarmHouseId,Amount=payment.Amount,CurrencyCode="INR",IssuedAt=DateTime.Now});await db.SaveChangesAsync();}
foreach(var key in new[]{"bookings","revenue","customers","occupancy"}) {
 await Test(key+" bounded list via canonical stored procedure",async()=>{var page=await reporting.ListAsync(key,new(){Period="all",PageSize=1},default);Check(page.Total>=0);});
 foreach(var format in new[]{"pdf","excel"})await Test(key+" "+format+" export",async()=>{var file=await reporting.ExportAsync(key,new(){Period="all"},format,default);Check(format=="pdf"?Encoding.ASCII.GetString(file.Content,0,4)=="%PDF":file.Content[0]==80&&file.Content[1]==75);Directory.CreateDirectory(".build-phase2/evidence");await File.WriteAllBytesAsync(".build-phase2/evidence/"+key+"."+(format=="pdf"?"pdf":"xlsx"),file.Content);});
}
await Test("Collections exclude other farmhouse payments",async()=>{var page=await reporting.ListAsync("revenue",new(){Period="all"},default);var json=System.Text.Json.JsonSerializer.Serialize(page.UntypedRows);Check(json.Contains("300")&&!json.Contains("900"));});
var receiptService=scope.ServiceProvider.GetRequiredService<IBookingReceipts>();
var firstBooking=await db.Bookings.FirstAsync(b=>b.FarmHouseId==farms[0].FarmHouseId);var firstReceipt=await db.BookingReceipts.FirstAsync(r=>r.FarmHouseId==farms[0].FarmHouseId);
await Test("Receipt renders using EF Core FromSql",async()=>{var file=await receiptService.RenderAsync(farms[0].FarmHouseId,firstBooking.BookingId,firstReceipt.ReceiptId,user.UserId,default);Check(file!=null&&Encoding.ASCII.GetString(file.Content,0,4)=="%PDF");});
await Test("Cross-tenant receipt blocked before SQL",()=>Denied(async()=>await receiptService.ListAsync(farms[1].FarmHouseId,firstBooking.BookingId,user.UserId,default)));
await Test("Cross-tenant report access blocked",()=>Denied(async()=>await scope.ServiceProvider.GetRequiredService<IReportAccess>().PermissionsAsync(farms[1].FarmHouseId,user.UserId,default)));
await Test("Collection title does not claim earned revenue",()=>{Check(ReportCatalog.Find("revenue").Title=="Booking Collections Report");return Task.CompletedTask;});
await Test("View-only report membership cannot export",async()=>{
 var grant=await db.RoleModulePermissions.SingleAsync(g=>g.RoleModule!.Module!.ModuleName=="Reporting" && g.RoleModule.RoleId==role.RoleId && g.Permission!.PermissionName=="RevenueReport.Export");
 grant.IsAllowed=false;await db.SaveChangesAsync();
 await using var viewScope=provider.CreateAsyncScope();var service=viewScope.ServiceProvider.GetRequiredService<IReportingService>();
 Check((await service.CatalogAsync(default)).Single(x=>x.Key=="revenue").CanExport==false);
 await service.ListAsync("revenue",new(){Period="all"},default);
 await Denied(async()=>await service.ExportAsync("revenue",new(){Period="all"},"pdf",default));
 grant.IsAllowed=true;await db.SaveChangesAsync();
});
await Test("Inactive membership cannot read reporting",async()=>{
 membership.IsActive=false;await db.SaveChangesAsync();
 await using var deniedScope=provider.CreateAsyncScope();await Denied(async()=>await deniedScope.ServiceProvider.GetRequiredService<IReportingService>().CatalogAsync(default));
 membership.IsActive=true;await db.SaveChangesAsync();
});
await Test("Report dates and pagination reject invalid filters",async()=>{
 foreach(var filter in new[]{new ReportFilter{Period="all",PageSize=101},new ReportFilter{Period="range",FromDate=new(2026,2,2),ToDate=new(2026,1,1)}}){try{await reporting.ListAsync("revenue",filter,default);throw new Exception("Expected invalid filter rejection");}catch(FluentValidation.ValidationException){}}
});
if(args.Contains("--http")) {
 var info=new System.Diagnostics.ProcessStartInfo("dotnet"){WorkingDirectory=Path.GetFullPath("FarmStay"),UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
 info.ArgumentList.Add(Path.GetFullPath("FarmStay/bin/Accounts/net8.0/FarmStay.API.dll"));info.ArgumentList.Add("--urls");info.ArgumentList.Add("http://127.0.0.1:7194");
 info.Environment["ASPNETCORE_ENVIRONMENT"]="Development";info.Environment["ConnectionStrings__DefaultConnection"]=builder.ConnectionString;
 using var server=System.Diagnostics.Process.Start(info)!;server.OutputDataReceived+=(_,_)=>{};server.ErrorDataReceived+=(_,_)=>{};server.BeginOutputReadLine();server.BeginErrorReadLine();
 try {
  using var client=new HttpClient{BaseAddress=new Uri("http://127.0.0.1:7194"),Timeout=TimeSpan.FromSeconds(45)};
  for(int n=0;n<40;n++){try{using var ready=await client.GetAsync("api/administration/reports");break;}catch(HttpRequestException){await Task.Delay(500);}}
  await Test("HTTP anonymous reporting denied",async()=>Check((int)(await client.GetAsync("api/administration/reports")).StatusCode==401));
  string Token()=>new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(config["Jwt:Issuer"],config["Jwt:Audience"],ctx.User.Claims,DateTime.Now.AddMinutes(-1),DateTime.Now.AddMinutes(10),new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256)));
  client.DefaultRequestHeaders.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",Token());
  await Test("HTTP administration reporting catalog",async()=>Check((int)(await client.GetAsync("api/administration/reports")).StatusCode==200));
  await Test("HTTP canonical report list",async()=>Check((int)(await client.GetAsync("api/administration/reports/revenue?period=all")).StatusCode==200));
  await Test("HTTP PDF export",async()=>{using var response=await client.GetAsync("api/administration/reports/revenue/export/pdf?period=all");Check((int)response.StatusCode==200);Check(response.Content.Headers.ContentType?.MediaType=="application/pdf");});
  await Test("HTTP tenant header spoof rejected",async()=>{client.DefaultRequestHeaders.Add("FarmHouseId",farms[1].FarmHouseId.ToString());Check((int)(await client.GetAsync("api/administration/reports")).StatusCode==403);client.DefaultRequestHeaders.Remove("FarmHouseId");});
  await Test("HTTP tenant query spoof rejected",async()=>Check((int)(await client.GetAsync("api/administration/reports/revenue?period=all&FarmHouseId="+farms[1].FarmHouseId)).StatusCode==403));
 } finally {if(!server.HasExited)server.Kill(true);await server.WaitForExitAsync();}
}
Console.WriteLine($"TOTAL passed={passed} failed={failed}");Environment.ExitCode=failed==0?0:1;

AppDbContext Db(string cs)=>new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(cs).Options);
FarmHouse Farm(string suffix,string mobile)=>new(){FarmHouseName="Report test "+suffix,DomainName="phase2-"+suffix+".invalid",Address="Test",District="Kolhapur",State="MH",Taluka="Karveer",ContactPersonName="Test",MobileNumber=mobile,Email="phase2-"+suffix+"@example.invalid"};
async Task<string> Snapshot(){var result="";foreach(var t in new[]{"Modules","RoleModules","RoleModulePermissions","FarmHouseModules"})result+=string.Join("",await db.Database.SqlQueryRaw<string>($"SELECT (SELECT * FROM dbo.{t} ORDER BY 1 FOR JSON PATH, INCLUDE_NULL_VALUES) Value").ToListAsync());return result;}
string Resource(string name){using var stream=typeof(AppDbContext).Assembly.GetManifestResourceStream("FarmStay.Infrastructure.SQL.Reports.Consolidation."+name+".sql")!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
sealed class FixedHttpContext:IHttpContextAccessor{public HttpContext? HttpContext{get;set;}}

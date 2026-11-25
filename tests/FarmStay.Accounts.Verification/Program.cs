using FarmStay.Application.Accounts;
using FarmStay.Domain.Accounts;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Accounts;
using FarmStay.Infrastructure.Data;
using FarmStay.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

int passed=0,failed=0;
async Task Test(string name,Func<Task> action){try{await action();passed++;Console.WriteLine("PASS "+name);}catch(Exception ex){failed++;Console.WriteLine("FAIL "+name+": "+ex.GetType().Name+" "+ex.Message);}}
void Check(bool ok,string message="Assertion failed"){if(!ok)throw new Exception(message);}
async Task Reject(Func<Task> action,int? status=null){try{await action();throw new Exception("Expected rejection");}catch(AccountsException e){if(status.HasValue)Check(e.Status==status);}catch(ArgumentException)when(!status.HasValue){}}
VoucherLine[] Pair(decimal amount)=>[new(){AccountId=1,Debit=amount},new(){AccountId=2,Credit=amount}];
await Test("Balanced double entry",()=>{AccountingRules.ValidateLines(Pair(100));return Task.CompletedTask;});
foreach(var amount in new[]{0m,-1m,1.001m,1000000000000m})await Test("Reject invalid money "+amount,()=>Reject(()=>{AccountingRules.Money(amount);return Task.CompletedTask;}));
await Test("Reject unbalanced voucher",()=>Reject(()=>{AccountingRules.ValidateLines([new(){Debit=10},new(){Credit=9}]);return Task.CompletedTask;}));
await Test("Reject both sides on one line",()=>Reject(()=>{AccountingRules.ValidateLines([new(){Debit=10,Credit=10},new(){Debit=1}]);return Task.CompletedTask;}));
await Test("Reject one-line voucher",()=>Reject(()=>{AccountingRules.ValidateLines([new(){Debit=10}]);return Task.CompletedTask;}));
await Test("SLM caps at residual",()=>{Check(AccountingRules.Depreciation(12000,2000,9900,10,"SLM",0)==100);return Task.CompletedTask;});
await Test("WDV uses configured rate",()=>{Check(AccountingRules.Depreciation(12000,0,2000,60,"WDV",12)==100);return Task.CompletedTask;});
await Test("Reject invalid depreciation",()=>Reject(()=>{AccountingRules.Depreciation(100,101,0,0,"SLM",0);return Task.CompletedTask;}));
await Test("Bound page size and sorting",()=>Reject(()=>{new AccountQuery{PageSize=10000,Sort="name; DROP TABLE"}.Validate();return Task.CompletedTask;},400));
if(args.Contains("--integration")){
 var cwd=Directory.GetCurrentDirectory();var config=new ConfigurationBuilder().AddJsonFile(Path.Combine(cwd,"FarmStay/appsettings.json"),optional:true).AddJsonFile(Path.Combine(cwd,"FarmStay/appsettings.Development.json"),optional:true).AddUserSecrets("78bc642e-7ece-4aad-b741-ab7b10bcb3f8").AddEnvironmentVariables().Build();
 var raw=config.GetConnectionString("DefaultConnection")??throw new Exception("Development database configuration is missing.");
 var reuse=args.FirstOrDefault(x=>x.StartsWith("--reuse-test="))?.Split('=',2)[1];
 if(reuse!=null&&!System.Text.RegularExpressions.Regex.IsMatch(reuse,"^FSAccounts_Test_[a-f0-9]{32}$"))throw new Exception("Only an explicit disposable verification database may be reused.");
 var builder=new SqlConnectionStringBuilder(raw){InitialCatalog=reuse??"FSAccounts_Test_"+Guid.NewGuid().ToString("N"),TrustServerCertificate=true};
 Check(builder.InitialCatalog.StartsWith("FSAccounts_Test_"));Console.WriteLine("Disposable test database: "+builder.InitialCatalog);
 var options=new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(builder.ConnectionString).Options;
 await using var db=new AppDbContext(options);
 if(args.Contains("--reports-only")) {
  var farm=await db.FarmHouses.OrderBy(x=>x.FarmHouseId).FirstAsync();var scope=new AccountScope(farm.FarmHouseId,0,"report-smoke",new HashSet<string>());var repoOnly=new AccountsRepository(db,NullLogger<AccountsRepository>.Instance);
  foreach(var key in AccountsRepository.Reports) {
   if(key=="tax"){await Test("GST report stays disabled",()=>Reject(async()=>await repoOnly.ReportAsync(scope,key,new(),false,CancellationToken.None),400));continue;}
   await Test("Live SP report: "+key,async()=>{var rows=await repoOnly.ReportAsync(scope,key,new(){From=DateTime.Today.AddMonths(-2),To=DateTime.Today,PageSize=100},false,CancellationToken.None);Check(rows.Count<=100);});
  }
  await Test("Updated professional PDF and XLSX",async()=>{foreach(var format in new[]{"pdf","excel"}){var file=await repoOnly.ExportAsync(scope,"trial-balance",new(){PageSize=100},format,CancellationToken.None);await File.WriteAllBytesAsync("docs/Accounts/evidence/trial-balance."+(format=="pdf"?"pdf":"xlsx"),file.Content);Check(file.Content.Length>1000);}});
  Console.WriteLine($"TOTAL passed={passed} failed={failed}");return failed==0?0:1;
 }
 await Test("Clean migrations",async()=>await db.Database.MigrateAsync());
 if(!await db.Database.CanConnectAsync())throw new Exception("Disposable database is unavailable.");
 var farms=new[]{new FarmHouse{FarmHouseName="Accounts test A",DomainName="accounts-test-a.invalid",Address="Test A",District="Kolhapur",State="MH",Taluka="Karveer",ContactPersonName="Test",MobileNumber="9000000001",Email="test-a@example.invalid"},new FarmHouse{FarmHouseName="Accounts test B",DomainName="accounts-test-b.invalid",Address="Test B",District="Kolhapur",State="MH",Taluka="Karveer",ContactPersonName="Test",MobileNumber="9000000002",Email="test-b@example.invalid"}};
 db.AddRange(farms);await db.SaveChangesAsync();var repo=new AccountsRepository(db,NullLogger<AccountsRepository>.Instance);var ct=CancellationToken.None;
 var a=new AccountScope(farms[0].FarmHouseId,0,"test-a",new HashSet<string>());var b=new AccountScope(farms[1].FarmHouseId,0,"test-b",new HashSet<string>());
 await Test("Tenant initialization and seed rerun",async()=>{await repo.InitializeAsync(a.FarmHouseId,0,ct);await repo.InitializeAsync(b.FarmHouseId,0,ct);var count=await db.Set<AccountingMaster>().CountAsync();await repo.InitializeAsync(a.FarmHouseId,0,ct);Check(count==await db.Set<AccountingMaster>().CountAsync());Check(await db.Set<LedgerAccount>().CountAsync(x=>x.FarmHouseId==a.FarmHouseId)==AccountsRepository.Chart.Length,"Tenant chart does not contain all required codes");});
 long A(string code)=>db.Set<LedgerAccount>().Single(x=>x.FarmHouseId==a.FarmHouseId&&x.Code==code).Id;
 long B(string code)=>db.Set<LedgerAccount>().Single(x=>x.FarmHouseId==b.FarmHouseId&&x.Code==code).Id;
 async Task<Voucher> Draft(string type,decimal amount,long debit,long credit,long? settles=null,long? vendor=null)=> (Voucher)await repo.SaveAsync(a,"vouchers",null,new VoucherInput{Type=type,Date=DateTime.Today,Narration="Verification "+type,SettlesVoucherId=settles,Lines=[new(){AccountId=debit,Debit=amount,VendorId=vendor},new(){AccountId=credit,Credit=amount,VendorId=vendor}]},ct);
 async Task<Voucher> Post(Voucher v)=> (Voucher)await repo.PostAsync(a,v.Id,new(){Version=v.Version},false,ct);
 Voucher? expense=null;
 await Test("Paid expense posts and reconciles",async()=>{expense=await Post(await Draft("Expense",100,A("6000"),A("1000")));Check(expense.Status=="Posted");var tb=await repo.ReportAsync(a,"trial-balance",new(){PageSize=100},false,ct);Check(tb.Sum(x=>x.Debit)==tb.Sum(x=>x.Credit));});
 await Test("Cross-tenant voucher ID denied",()=>Reject(async()=>await repo.VoucherAsync(b,expense!.Id,ct),404));
 await Test("Cross-tenant account FK rejected",()=>Reject(async()=>await Draft("Journal",10,A("1000"),B("3000")),400));db.ChangeTracker.Clear();
 await Test("System master modification denied",async()=>{var m=await db.Set<AccountingMaster>().FirstAsync();await Reject(async()=>await repo.SaveAsync(a,"masters",m.Id,new MasterInput{Kind=m.Kind,Code=m.Code,Name="Attack"},ct));});db.ChangeTracker.Clear();
 await Test("Posted voucher cannot be edited",()=>Reject(async()=>await repo.SaveAsync(a,"vouchers",expense!.Id,new VoucherInput{Date=DateTime.Today,Type="Expense",Narration="Attack",Version=expense.Version,Lines=[new(){AccountId=A("6000"),Debit=1},new(){AccountId=A("1000"),Credit=1}]},ct),409));db.ChangeTracker.Clear();
 await Test("Posted immutability enforced by database",async()=>{try{await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.AccountingVouchers SET Narration=N'Attack' WHERE Id={expense!.Id}");throw new Exception("Expected trigger rejection");}catch(SqlException e){Check(e.Number==51001);}});
 await Test("Closed-period posting rejected",async()=>{var p=await db.Set<AccountingPeriod>().SingleAsync(x=>x.FarmHouseId==a.FarmHouseId&&x.FromDate<=DateTime.Today&&x.ToDate>=DateTime.Today);await repo.SaveAsync(a,"periods",p.Id,new ActionInput{Amount=1,Reason="Test close",Version=p.Version},ct);await Reject(async()=>await Draft("Journal",10,A("1000"),A("3000")));db.ChangeTracker.Clear();p=await db.Set<AccountingPeriod>().SingleAsync(x=>x.Id==p.Id);await repo.SaveAsync(a,"periods",p.Id,new ActionInput{Amount=0,Reason="Test reopen",Version=p.Version},ct);});
 await Test("Stale voucher version rejected",async()=>{var v=await Draft("Journal",10,A("1000"),A("3000"));await Reject(async()=>await repo.PostAsync(a,v.Id,new(){Version=[1,2,3]},false,ct),409);});db.ChangeTracker.Clear();
 await Test("Reverse creates opposite entry",async()=>{var v=await db.Set<Voucher>().Include(x=>x.Lines).SingleAsync(x=>x.Id==expense!.Id);var rev=(Voucher)await repo.PostAsync(a,v.Id,new(){Version=v.Version,Date=DateTime.Today,Reason="Verification reversal"},true,ct);Check(rev.ReversalOfVoucherId==v.Id&&rev.TotalDebit==v.TotalCredit);});
 await Test("GST-disabled tax report rejected",()=>Reject(async()=>await repo.ReportAsync(a,"tax",new(),false,ct),400));
 await Test("Vendor bill and partial payment ageing",async()=>{var v=(Vendor)await repo.SaveAsync(a,"vendors",null,new VendorInput{Code="TESTV",Name="Verification vendor"},ct);var bill=await Post(await Draft("VendorBill",1000,A("6040"),v.AccountId,null,v.Id));await Post(await Draft("VendorPayment",250,v.AccountId,A("1000"),bill.Id,v.Id));var report=await repo.ReportAsync(a,"payables",new(){PageSize=100},false,ct);Check(report.Single(x=>x.Reference==bill.VoucherNumber).Balance==750);});
 await Test("Cross-tenant attachment denied",()=>Reject(async()=>await repo.AttachmentAsync(b,expense!.Id,"bad.pdf","application/pdf",[37,80,68,70,1],ct),404));
 await Test("Unsafe upload rejected",async()=>{var v=await Draft("Journal",5,A("1000"),A("3000"));await Reject(async()=>await repo.AttachmentAsync(a,v.Id,"attack.html","text/html",[60,62],ct),400);});

 await Test("Asset acquisition, depreciation, maintenance and disposal",async()=>{
  var start=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(-1);var category=await db.Set<AccountingMaster>().FirstAsync(x=>x.Kind=="AssetCategory");
  var asset=(FixedAsset)await repo.SaveAsync(a,"assets",null,new AssetInput{Code="TEST-ASSET",Name="Verification asset",CategoryId=category.Id,PurchaseDate=start,InServiceDate=start,Cost=12000,ResidualValue=0,UsefulLifeMonths=12,DepreciationMethod="SLM",CreditAccountId=A("1000"),Location="Test"},ct);
  await repo.AssetActionAsync(a,asset.Id,"depreciation",new(){Date=start.AddMonths(1).AddDays(-1),Reason="Monthly verification",Version=asset.Version},ct);
  db.ChangeTracker.Clear();asset=await db.Set<FixedAsset>().SingleAsync(x=>x.Id==asset.Id);await repo.AssetActionAsync(a,asset.Id,"maintenance",new(){Date=DateTime.Today,Reason="Maintenance verification",Amount=100,AccountId=A("1000"),Version=asset.Version},ct);
  db.ChangeTracker.Clear();asset=await db.Set<FixedAsset>().SingleAsync(x=>x.Id==asset.Id);await repo.AssetActionAsync(a,asset.Id,"disposal",new(){Date=DateTime.Today,Reason="Disposal verification",Amount=11500,AccountId=A("1000"),Version=asset.Version},ct);
  Check((await db.Set<FixedAsset>().SingleAsync(x=>x.Id==asset.Id)).Status=="Disposed");
 });
 var customer=new User{FarmHouseId=a.FarmHouseId,FullName="Verification customer",MobileNumber="9000000099",PasswordHash="test-only-no-login"};db.Add(customer);await db.SaveChangesAsync();var role=await db.Roles.FirstOrDefaultAsync(x=>x.RoleName=="Customer");if(role==null){role=new Role{RoleName="Customer"};db.Add(role);await db.SaveChangesAsync();}var membership=new UserMembership{FarmHouseId=a.FarmHouseId,UserId=customer.UserId,RoleId=role.RoleId};db.Add(membership);await db.SaveChangesAsync();
 var booking=new Booking{FarmHouseId=a.FarmHouseId,CustomerUserId=customer.UserId,BookingReference="TEST-ACCOUNTING",FullName="Test",MobileNumber="9000000099",AdultCount=1,PersonCount=1,CheckInDate=DateTime.Today,CheckOutDate=DateTime.Today.AddDays(1),ActualCheckOutAt=DateTime.Now,StayDays=1,TotalAmount=1000,BaseAmount=1000,Status="CheckedOut",CreatedBy=customer.UserId};db.Add(booking);await db.SaveChangesAsync();db.AddRange(new PaymentTransaction{FarmHouseId=a.FarmHouseId,BookingId=booking.BookingId,PaymentType="Advance",PaymentMethod="Cash",Amount=300,PaymentStatus="Success",IdempotencyKey="account-test-advance",PaidAt=DateTime.Now},new PaymentTransaction{FarmHouseId=a.FarmHouseId,BookingId=booking.BookingId,PaymentType="Checkout",PaymentMethod="Cash",Amount=700,PaymentStatus="Success",IdempotencyKey="account-test-checkout",PaidAt=DateTime.Now});await db.SaveChangesAsync();
 await Test("Booking advance + fulfilment + final receipt are idempotent",async()=>{await repo.ProcessSourcesAsync(ct);var first=await db.Set<Voucher>().CountAsync(x=>x.FarmHouseId==a.FarmHouseId&&x.SourceKey!=null);await repo.ProcessSourcesAsync(ct);Check(first==await db.Set<Voucher>().CountAsync(x=>x.FarmHouseId==a.FarmHouseId&&x.SourceKey!=null));Check(await db.Set<Voucher>().CountAsync(x=>x.FarmHouseId==a.FarmHouseId&&x.SourceKey=="BookingRevenue:"+booking.BookingId)==1);});
 await Test("Accounting equation and trial balance reconcile",async()=>{var rows=await repo.ReportAsync(a,"balance-sheet",new(){PageSize=100},false,ct);Check(rows.Sum(x=>x.Balance)==0);var tb=await repo.ReportAsync(a,"trial-balance",new(){PageSize=100},false,ct);Check(tb.Sum(x=>x.Balance)==0);});
 await Test("Cross-tenant ledger export denied",()=>Reject(async()=>await repo.ExportAsync(b,"ledger",new(){AccountId=A("1000")},"pdf",ct),404));
 await Test("PDF and XLSX render",async()=>{var pdf=await repo.ExportAsync(a,"trial-balance",new(){PageSize=100},"pdf",ct);Check(System.Text.Encoding.ASCII.GetString(pdf.Content,0,4)=="%PDF");var excel=await repo.ExportAsync(a,"trial-balance",new(){PageSize=100},"excel",ct);Check(excel.Content[0]==80&&excel.Content[1]==75);Directory.CreateDirectory("docs/Accounts/evidence");await File.WriteAllBytesAsync("docs/Accounts/evidence/trial-balance.pdf",pdf.Content);await File.WriteAllBytesAsync("docs/Accounts/evidence/trial-balance.xlsx",excel.Content);});
 var ctx=new DefaultHttpContext();ctx.User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,customer.UserId.ToString()),new Claim("UserMembershipId",membership.UserMembershipId.ToString()),new Claim("FarmHouseId",a.FarmHouseId.ToString())],"test"));ctx.Request.Headers["FarmHouseId"]=b.FarmHouseId.ToString();var access=new AccountsScope(new FixedHttpContextAccessor{HttpContext=ctx},db);

 await Test("Tenant authority comes from membership, not header or legacy User.FarmHouseId",async()=>{
  var adminRole=new Role{RoleName="Admin"};db.Add(adminRole);await db.SaveChangesAsync();
  using(var stream=typeof(AccountsRepository).Assembly.GetManifestResourceStream("FarmStay.Infrastructure.Accounts.Sql.AccountsSeeds.sql"))using(var reader=new StreamReader(stream!))await db.Database.ExecuteSqlRawAsync(await reader.ReadToEndAsync());
  var adminUser=new User{FarmHouseId=b.FarmHouseId,FullName="Membership authority test",MobileNumber="9000000088",PasswordHash="test-only-no-login"};db.Add(adminUser);await db.SaveChangesAsync();var adminMember=new UserMembership{FarmHouseId=a.FarmHouseId,UserId=adminUser.UserId,RoleId=adminRole.RoleId};db.Add(adminMember);await db.SaveChangesAsync();
  var adminCtx=new DefaultHttpContext();adminCtx.User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,adminUser.UserId.ToString()),new Claim("UserMembershipId",adminMember.UserMembershipId.ToString())],"test"));adminCtx.Request.Headers["FarmHouseId"]=b.FarmHouseId.ToString();
  var resolved=await new AccountsScope(new FixedHttpContextAccessor{HttpContext=adminCtx},db).RequireAsync("Accounts.View",ct);Check(resolved.FarmHouseId==a.FarmHouseId);Check(resolved.UserId==adminUser.UserId);
 });

 await Test("All sensitive permissions reject an ungranted membership",async()=>{
  foreach(var permission in new[]{"Create","Edit","Post","Reverse","Export","ManageMasters","ManagePeriods","ManageAssets"}) await Reject(async()=>await access.RequireAsync("Accounts."+permission,ct),403);
 });
 await Test("Anonymous Accounts scope is rejected",()=>Reject(async()=>await new AccountsScope(new FixedHttpContextAccessor{HttpContext=new DefaultHttpContext()},db).RequireAsync("Accounts.View",ct),401));
 if(args.Contains("--http")) {
  var apiPath=Path.Combine(cwd,"FarmStay/bin/Accounts/net8.0/FarmStay.API.dll");
  var startInfo=new ProcessStartInfo("dotnet"){WorkingDirectory=Path.Combine(cwd,"FarmStay"),UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};startInfo.ArgumentList.Add(apiPath);startInfo.ArgumentList.Add("--urls");startInfo.ArgumentList.Add("http://127.0.0.1:7193");startInfo.Environment["ASPNETCORE_ENVIRONMENT"]="Development";startInfo.Environment["ConnectionStrings__DefaultConnection"]=builder.ConnectionString;
  using var server=Process.Start(startInfo)??throw new Exception("Could not start isolated API");server.OutputDataReceived+=(_,_)=>{};server.ErrorDataReceived+=(_,_)=>{};server.BeginOutputReadLine();server.BeginErrorReadLine();
  try{
   using var client=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=new Uri("http://127.0.0.1:7193"),Timeout=TimeSpan.FromSeconds(45)};
   for(var attempt=0;attempt<40;attempt++){try{using var ready=await client.GetAsync("/api/administration/accounts/access");break;}catch(HttpRequestException){await Task.Delay(500);}}
   await Test("HTTP anonymous access returns 401",async()=>{using var response=await client.GetAsync("/api/administration/accounts/access");Check(response.StatusCode==HttpStatusCode.Unauthorized,"Expected 401, received "+response.StatusCode);});
   string Token(UserMembership member)=>new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(config["Jwt:Issuer"],config["Jwt:Audience"],[new Claim(ClaimTypes.NameIdentifier,member.UserId.ToString()),new Claim("UserMembershipId",member.UserMembershipId.ToString()),new Claim("FarmHouseId",member.FarmHouseId.ToString())],DateTime.Now.AddMinutes(-1),DateTime.Now.AddMinutes(10),new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),SecurityAlgorithms.HmacSha256)));
   client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",Token(membership));
   await Test("HTTP customer cannot access Accounts",async()=>{using var response=await client.GetAsync("/api/administration/accounts/vouchers");Check(response.StatusCode==HttpStatusCode.Forbidden,"Expected 403, received "+response.StatusCode);});
   var adminMember=await db.UserMemberships.Include(x=>x.Role).SingleAsync(x=>x.FarmHouseId==a.FarmHouseId&&x.Role!.RoleName=="Admin");client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",Token(adminMember));client.DefaultRequestHeaders.Add("FarmHouseId",b.FarmHouseId.ToString());
   await Test("HTTP forged tenant header cannot select another tenant",async()=>{using var response=await client.GetAsync("/api/administration/accounts/ledgers?pageSize=100");Check(response.StatusCode==HttpStatusCode.OK,"Expected 200, received "+response.StatusCode);using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync());Check(json.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().All(x=>x.GetProperty("farmHouseId").GetInt32()==a.FarmHouseId));});
   await Test("HTTP rejects excessive pagination",async()=>{using var response=await client.GetAsync("/api/administration/accounts/vouchers?pageSize=10000");Check(response.StatusCode==HttpStatusCode.BadRequest);});
  }finally{if(!server.HasExited)server.Kill(true);await server.WaitForExitAsync();}
 }
 await Test("View-only/customer cannot escalate to posting",()=>Reject(async()=>await access.RequireAsync("Accounts.Post",ct),403));
 await Test("Invalid membership denied",async()=>{membership.IsActive=false;db.Update(membership);await db.SaveChangesAsync();await Reject(async()=>await access.RequireAsync("Accounts.View",ct),403);});
 Console.WriteLine("Test DB retained for evidence: "+builder.InitialCatalog);
}
Console.WriteLine($"TOTAL passed={passed} failed={failed}");return failed==0?0:1;

sealed class FixedHttpContextAccessor : IHttpContextAccessor { public HttpContext? HttpContext { get; set; } }

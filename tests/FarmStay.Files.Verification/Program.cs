using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Domain.Accounts;
using FarmStay.Infrastructure.Data;
using FarmStay.Infrastructure.Repositories.Booking;
using FarmStay.Infrastructure.Services.FileUpload;
using FarmStay.Infrastructure.Accounts;
using FarmStay.Application.Accounts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text.Json;

int passed=0,failed=0;
void Check(bool value){if(!value)throw new Exception("Assertion failed");}
async Task Test(string name,Func<Task> action){try{await action();passed++;Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
async Task Reject(Func<Task> action){try{await action();throw new Exception("Expected rejection");}catch(ArgumentException){}catch(AccountsException){}catch(DbUpdateException){}catch(InvalidOperationException){}}
var config=new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("FarmStay/appsettings.json"),true).AddJsonFile(Path.GetFullPath("FarmStay/appsettings.Development.json"),true).AddUserSecrets("78bc642e-7ece-4aad-b741-ab7b10bcb3f8").AddEnvironmentVariables().Build();
var connection=config.GetConnectionString("DefaultConnection")!;
AppDbContext Db(string c)=>new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(c).Options);
if(args.Contains("--apply")){await using var live=Db(connection);await live.Database.MigrateAsync();Console.WriteLine("Phase 5 migration applied.");return;}
var sql=new SqlConnectionStringBuilder(connection){InitialCatalog="FSFiles_Phase5_"+Guid.NewGuid().ToString("N")};Console.WriteLine("Disposable database: "+sql.InitialCatalog);
await using var db=Db(sql.ConnectionString);await db.Database.MigrateAsync();
await Test("Clean migration and structural rollback preserve file tables",async()=>{await db.GetService<IMigrator>().MigrateAsync("20261126000100_StabilizeAccountsFinance");await db.Database.MigrateAsync();});
var storage=Path.Combine(Path.GetTempPath(),"FarmStay_Phase5_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(storage);Console.WriteLine("Isolated storage: "+storage);
byte[] jpg=[255,216,255,1,2,3];byte[] back=[255,216,255,4,5,6];
Task<string> Store(int farm=1,string category="Gallery",byte[]? bytes=null,long max=100)=>FileSafety.StorePublicAsync(new MemoryStream(bytes??jpg),storage,farm,category,".jpg",max,default);
string? first=null;
await Test("Public validated content stores under tenant and category",async()=>{first=await Store();Check(File.Exists(Path.Combine(storage,"wwwroot","uploads","1","Gallery",first)));});
await Test("Identical public retry reuses its original URL",async()=>Check(first==await Store()));
await Test("Concurrent duplicate public uploads create one asset",async()=>{var results=await Task.WhenAll(Enumerable.Range(0,6).Select(_=>Store()));Check(results.Distinct().Count()==1&&Directory.GetFiles(Path.Combine(storage,"wwwroot","uploads","1","Gallery")).Length==1);});
await Test("Public dedup never shares storage across tenants or categories",async()=>{await Store(2);await Store(1,"Amenity");Check(Directory.GetFiles(Path.Combine(storage,"wwwroot","uploads"),"*.jpg",SearchOption.AllDirectories).Length==3);});
await Test("Oversized upload removes staging and keeps existing references",async()=>{await Reject(async()=>await Store(bytes:new byte[101]));Check(File.ReadAllBytes(Path.Combine(storage,"wwwroot","uploads","1","Gallery",first!)).SequenceEqual(jpg));Check(Directory.GetFiles(Path.Combine(storage,"App_Data","UploadStaging")).Length==0);});
await Test("Empty and invalid destination uploads are rejected",async()=>{await Reject(async()=>await Store(bytes:[]));await Reject(async()=>await Store(category:"../private"));await Reject(async()=>await Store(0));});
await Test("Corrupted public dedup target is never silently reused",async()=>{var file=Path.Combine(storage,"wwwroot","uploads","1","Gallery",first!);await File.WriteAllBytesAsync(file,[1]);try{await Store();throw new Exception("Expected integrity rejection");}catch(IOException){}finally{await File.WriteAllBytesAsync(file,jpg);}});
await Test("Private root cannot be public webroot or its descendant",()=>{foreach(var path in new[]{Path.Combine(storage,"wwwroot"),Path.Combine(storage,"wwwroot","private")}){try{FileSafety.PrivateRoot(path,Path.Combine(storage,"wwwroot"));throw new Exception("Unsafe root accepted");}catch(InvalidOperationException){}}Check(FileSafety.PrivateRoot(Path.Combine(storage,"private"),Path.Combine(storage,"wwwroot")).EndsWith("private"));return Task.CompletedTask;});
await Test("Private names sanitize paths and reject executable double extension",async()=>{Check(FileSafety.DisplayName("a/b/front.jpg")=="front.jpg");await Reject(()=>{FileSafety.DisplayName("attack.php.jpg");return Task.CompletedTask;});});
await Test("Private format validates matching extension MIME and signature",()=>{Check(FileSafety.ValidProof("id.jpg","image/jpeg",jpg));Check(!FileSafety.ValidProof("id.png","image/jpeg",jpg));Check(!FileSafety.ValidProof("id.jpg","image/jpeg",[60,62]));Check(!FileSafety.ValidProof("id.pdf","application/pdf",[37,80,68,70,1]));return Task.CompletedTask;});
await Test("Public repository enforces exact MIME signature and bounded size",async()=>{
    var context=new Microsoft.AspNetCore.Http.DefaultHttpContext();context.Request.Scheme="https";context.Request.Host=new Microsoft.AspNetCore.Http.HostString("files.invalid");
    var settings=Microsoft.Extensions.Options.Options.Create(new FarmStay.Application.Common.Settings.FileUploadSettings{AllowedImageExtensions=[".jpg",".png"],AllowedImageMimeTypes=["image/jpeg","image/png"],MaxImageSizeInBytes=100});
    var repository=new FarmStay.Infrastructure.Services.Common.FileUploadRepository(new Host{ContentRootPath=storage},new Microsoft.AspNetCore.Http.HttpContextAccessor{HttpContext=context},NullLogger<FarmStay.Infrastructure.Services.Common.FileUploadRepository>.Instance,settings,new Tenant());
    async Task<bool> Upload(string name,string mime,byte[] bytes){var file=new Microsoft.AspNetCore.Http.FormFile(new MemoryStream(bytes),0,bytes.Length,"File",name){Headers=new Microsoft.AspNetCore.Http.HeaderDictionary(),ContentType=mime};return (await repository.UploadFileAsync(new(){Folder="Gallery",File=file})).Success;}
    Check(!await Upload("photo.jpg","image/png",jpg));Check(!await Upload("photo.jpg","image/jpeg",[60,62]));Check(!await Upload("photo.php.jpg","image/jpeg",jpg));Check(!await Upload("photo.jpg","image/jpeg",new byte[101]));Check(await Upload("photo.jpg","image/jpeg",jpg));
});
FarmHouse Farm(string suffix)=>new(){FarmHouseName="Files "+suffix,DomainName="files-"+suffix+".invalid",Address="Fixture",District="Kolhapur",State="MH",Taluka="Karveer",ContactPersonName="Fixture",MobileNumber="900000050"+suffix};
var fa=Farm("1");var fb=Farm("2");db.AddRange(fa,fb);await db.SaveChangesAsync();
var customer=new User{FarmHouseId=fa.FarmHouseId,FullName="File Customer",MobileNumber="9000000503",PasswordHash="fixture"};var second=new User{FarmHouseId=fa.FarmHouseId,FullName="Other Customer",MobileNumber="9000000504",PasswordHash="fixture"};db.AddRange(customer,second);await db.SaveChangesAsync();
var booking=new Booking{FarmHouseId=fa.FarmHouseId,CustomerUserId=customer.UserId,FullName=customer.FullName,MobileNumber=customer.MobileNumber,BookingReference="FILES-1",AdultCount=1,PersonCount=1,CheckInDate=DateTime.Today,CheckOutDate=DateTime.Today.AddDays(1),StayDays=1,Status="Approved",CustomerDeadline=DateTime.Now.AddHours(24),OwnerDeadline=DateTime.Now.AddHours(48),CreatedBy=customer.UserId};db.Add(booking);await db.SaveChangesAsync();
await Test("Upgrade preflight rejects inconsistent legacy data without deleting it",async()=>{
    await db.GetService<IMigrator>().MigrateAsync("20261126000100_StabilizeAccountsFinance");
    var bad=new BookingDocument{FarmHouseId=fb.FarmHouseId,BookingId=booking.BookingId,UserId=customer.UserId,DocumentType="IdentityProof",StorageKey=Guid.NewGuid().ToString("N"),OriginalFileName="legacy.jpg",ContentType="image/jpeg",Sha256Hash=new string('0',64)};
    db.Add(bad);await db.SaveChangesAsync();
    try{await db.Database.MigrateAsync();throw new Exception("Preflight accepted invalid ownership");}catch(SqlException e){Check(e.Number==51033);}
    Check(await db.BookingDocuments.AnyAsync(d=>d.BookingDocumentId==bad.BookingDocumentId));
    // Remove only this isolated test fixture; no production remediation is performed.
    db.Remove(bad);await db.SaveChangesAsync();await db.Database.MigrateAsync();db.ChangeTracker.Clear();
});
var privateConfig=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"BookingDocuments:PrivateRoot",Path.Combine(storage,"private")}}).Build();
BookingIdentityDocuments Identity(AppDbContext context)=>new(context,privateConfig,new Notifications(),new Host{ContentRootPath=storage});
var identity=Identity(db);IdentityProofUpload Front(byte[]? bytes=null)=>new("front.jpg","image/jpeg",bytes??jpg);
BookingIdentityDocument? uploaded=null;
await Test("Duplicate front/back files are rejected before private writes",async()=>{Check(await identity.UploadAsync(fa.FarmHouseId,booking.BookingId,customer.UserId,"Aadhaar Card",null,[Front(),Front()],default)==null);Check(!Directory.Exists(Path.Combine(storage,"private")));});
await Test("Private upload preserves two files and identity type",async()=>{uploaded=await identity.UploadAsync(fa.FarmHouseId,booking.BookingId,customer.UserId,"Aadhaar Card",null,[Front(),new("back.jpg","image/jpeg",back)],default);Check(uploaded?.Files.Count==2&&uploaded.DocumentType=="Aadhaar Card");});
await Test("Lost-response retry returns original identity without extra files",async()=>{var replay=await identity.UploadAsync(fa.FarmHouseId,booking.BookingId,customer.UserId,"Aadhaar Card",null,[Front(),new("back.jpg","image/jpeg",back)],default);Check(replay?.Id==uploaded?.Id&&Directory.GetFiles(Path.Combine(storage,"private")).Length==2);});
await Test("Different upload cannot overwrite an accepted identity",async()=>Check(await identity.UploadAsync(fa.FarmHouseId,booking.BookingId,customer.UserId,"PAN Card",null,[Front(back)],default)==null));
await Test("Private metadata is isolated by tenant and customer",async()=>{Check(await identity.GetAsync(fb.FarmHouseId,booking.BookingId,null,default)==null);Check(await identity.GetAsync(fa.FarmHouseId,booking.BookingId,second.UserId,default)==null);Check(await identity.OpenAsync(fb.FarmHouseId,booking.BookingId,uploaded!.Id,default)==null);});
await Test("Private download verifies size and content hash",async()=>{var opened=await identity.OpenAsync(fa.FarmHouseId,booking.BookingId,uploaded!.Id,default);Check(opened!=null);await opened!.Value.Content.DisposeAsync();var record=await db.BookingDocuments.AsNoTracking().SingleAsync(d=>d.BookingDocumentId==uploaded.Id);var path=Path.Combine(storage,"private",record.StorageKey);await File.WriteAllBytesAsync(path,[1]);Check(await identity.OpenAsync(fa.FarmHouseId,booking.BookingId,uploaded.Id,default)==null);await File.WriteAllBytesAsync(path,jpg);});
await Test("Reupload keeps superseded audit bytes and creates a new current version",async()=>{await db.Bookings.Where(b=>b.BookingId==booking.BookingId).ExecuteUpdateAsync(u=>u.SetProperty(b=>b.Status,"Confirmed"));db.ChangeTracker.Clear();Check(await identity.ReviewAsync(fa.FarmHouseId,booking.BookingId,uploaded!.Id,customer.UserId,"Clearer photo",default));var replacement=await identity.UploadForCustomerAsync(fa.FarmHouseId,booking.BookingId,customer.UserId,"PAN Card",null,[Front(back)],default);Check(replacement!=null&&replacement.Id!=uploaded.Id&&await db.BookingDocuments.CountAsync(d=>d.BookingId==booking.BookingId&&d.VerificationStatus=="Superseded")==2&&Directory.GetFiles(Path.Combine(storage,"private")).Length==3);Check(await identity.OpenAsync(fa.FarmHouseId,booking.BookingId,uploaded.Id,default)==null);});
await Test("Document customer and parent tenant are enforced by SQL",async()=>{db.ChangeTracker.Clear();db.Add(new BookingDocument{FarmHouseId=fa.FarmHouseId,BookingId=booking.BookingId,UserId=second.UserId,DocumentType="IdentityProof",StorageKey=Guid.NewGuid().ToString("N"),OriginalFileName="id.jpg",ContentType="image/jpeg",Sha256Hash=new string('0',64)});await Reject(()=>db.SaveChangesAsync());db.ChangeTracker.Clear();});
var repo=new AccountsRepository(db,NullLogger<AccountsRepository>.Instance);var scope=new AccountScope(fa.FarmHouseId,customer.UserId,"files-test",new HashSet<string>());var otherScope=new AccountScope(fb.FarmHouseId,customer.UserId,"files-test-b",new HashSet<string>());await repo.InitializeAsync(fa.FarmHouseId,customer.UserId,default);
long Code(string code)=>db.Set<LedgerAccount>().Single(a=>a.FarmHouseId==fa.FarmHouseId&&a.Code==code).Id;
var voucher=(Voucher)await repo.SaveAsync(scope,"vouchers",null,new VoucherInput{Type="Journal",Date=DateTime.Today,Narration="File fixture",Lines=[new(){AccountId=Code("1000"),Debit=10},new(){AccountId=Code("3000"),Credit=10}]},default);
await Test("Accounting duplicate attachment returns original ID",async()=>{var one=JsonSerializer.Serialize(await repo.AttachmentAsync(scope,voucher.Id,"front.jpg","image/jpeg",jpg,default));var two=JsonSerializer.Serialize(await repo.AttachmentAsync(scope,voucher.Id,"renamed.jpg","image/jpeg",jpg,default));Check(one==two&&await db.Set<AccountingAttachment>().CountAsync(a=>a.VoucherId==voucher.Id)==1);});
await Test("Concurrent accounting duplicates create one attachment",async()=>{async Task Upload(){await using var ctx=Db(sql.ConnectionString);await new AccountsRepository(ctx,NullLogger<AccountsRepository>.Instance).AttachmentAsync(scope,voucher.Id,"back.jpg","image/jpeg",back,default);}await Task.WhenAll(Upload(),Upload());Check(await db.Set<AccountingAttachment>().CountAsync(a=>a.VoucherId==voucher.Id)==2);});
await Test("Attachment cannot reference another tenant voucher",async()=>{db.Add(new AccountingAttachment{FarmHouseId=fb.FarmHouseId,VoucherId=voucher.Id,FileName="id.jpg",ContentType="image/jpeg",Content=jpg});await Reject(()=>db.SaveChangesAsync());db.ChangeTracker.Clear();});
await Test("Unauthorized attachment download is denied",async()=>{var attachment=await db.Set<AccountingAttachment>().FirstAsync(a=>a.VoucherId==voucher.Id);await Reject(async()=>await repo.DownloadAttachmentAsync(otherScope,attachment.Id,default));});
await Test("Concurrent attachment limit remains five",async()=>{
    for(var i=0;i<2;i++)await repo.AttachmentAsync(scope,voucher.Id,"extra.jpg","image/jpeg",[255,216,255,(byte)(10+i)],default);
    async Task<bool> Add(byte n){await using var ctx=Db(sql.ConnectionString);try{await new AccountsRepository(ctx,NullLogger<AccountsRepository>.Instance).AttachmentAsync(scope,voucher.Id,"last.jpg","image/jpeg",[255,216,255,n],default);return true;}catch(AccountsException){return false;}}
    var outcomes=await Task.WhenAll(Add(40),Add(41));Check(outcomes.Count(x=>x)==1&&await db.Set<AccountingAttachment>().CountAsync(a=>a.VoucherId==voucher.Id)==5);
});
await Test("Posted voucher refuses new attachments",async()=>{
    var current=await db.Set<Voucher>().AsNoTracking().SingleAsync(v=>v.Id==voucher.Id);
    await repo.PostAsync(scope,voucher.Id,new(){Version=current.Version},false,default);
    await Reject(async()=>await repo.AttachmentAsync(scope,voucher.Id,"late.jpg","image/jpeg",jpg,default));
});
var media=new PublicMediaAccess(new PublicTenant(fa.FarmHouseId),db);
await Test("Public media boundary blocks tenant mismatch and unsafe paths",async()=>{Check(await media.CanReadAsync($"/uploads/{fa.FarmHouseId}/Gallery/photo.jpg"));Check(!await media.CanReadAsync($"/uploads/{fb.FarmHouseId}/Gallery/photo.jpg"));Check(!await media.CanReadAsync($"/uploads/{fa.FarmHouseId}/Gallery/../secret.pdf"));Check(!await media.CanReadAsync($"/uploads/{fa.FarmHouseId}/Private/id.jpg"));});
await Test("Separate API host resolves media from the mapped referring website",async()=>{
    var http=new Microsoft.AspNetCore.Http.DefaultHttpContext();http.Request.Host=new Microsoft.AspNetCore.Http.HostString("api.files.invalid");http.Request.Path=$"/uploads/{fa.FarmHouseId}/Gallery/photo.jpg";http.Request.Headers["Referer"]=$"https://{fa.DomainName}/gallery";
    var resolver=new FarmStay.Infrastructure.Services.Security.PublicTenantResolver(new FarmStay.Application.Services.Common.TenantResolver(new Microsoft.AspNetCore.Http.HttpContextAccessor{HttpContext=http}),db);
    var access=new PublicMediaAccess(resolver,db);Check(await access.CanReadAsync(http.Request.Path.Value!));
    http.Request.Headers["Referer"]=$"https://{fb.DomainName}/gallery";Check(!await access.CanReadAsync(http.Request.Path.Value!));
    http.Request.Headers["Referer"]="https://unknown-files.invalid/gallery";try{await access.CanReadAsync(http.Request.Path.Value!);throw new Exception("Unknown domain allowed");}catch(TenantAccessException){}
});
await Test("Legacy logo resolves only for its owning farmhouse",async()=>{await db.FarmHouses.Where(f=>f.FarmHouseId==fa.FarmHouseId).ExecuteUpdateAsync(u=>u.SetProperty(f=>f.LogoUrl,"https://files.invalid/uploads/logos/own.png"));Check(await media.CanReadAsync("/uploads/logos/own.png"));Check(!await media.CanReadAsync("/uploads/logos/other.png"));Check(await media.CanReadAsync($"/uploads/{fa.FarmHouseId}/FarmHouse/cover.jpg"));});
await Test("HTTP public media serves own asset and blocks cross-tenant and private paths",async()=>{
    var file=await Store(fa.FarmHouseId);
    var builder=Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();

    Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IPublicMediaAccess>(builder.Services,media);
    await using var app=builder.Build();app.Urls.Add("http://127.0.0.1:7196");
    Microsoft.AspNetCore.Builder.UseMiddlewareExtensions.UseMiddleware<FarmStay.API.Middleware.PublicMediaBoundaryMiddleware>(app);
    Microsoft.AspNetCore.Builder.StaticFileExtensions.UseStaticFiles(app,new Microsoft.AspNetCore.Builder.StaticFileOptions{FileProvider=new PhysicalFileProvider(Path.Combine(storage,"wwwroot"))});
    await app.StartAsync();try{
        using var http=new HttpClient{BaseAddress=new Uri("http://127.0.0.1:7196")};
        var own=await http.GetAsync($"/uploads/{fa.FarmHouseId}/Gallery/{file}");Check(own.IsSuccessStatusCode&&own.Headers.GetValues("X-Content-Type-Options").Single()=="nosniff");
        Check((await http.GetAsync($"/uploads/{fb.FarmHouseId}/Gallery/{file}")).StatusCode==System.Net.HttpStatusCode.NotFound);
        Check((await http.GetAsync($"/uploads/{fa.FarmHouseId}/Private/id.jpg")).StatusCode==System.Net.HttpStatusCode.NotFound);
    }finally{await app.StopAsync();}
});
Console.WriteLine($"TOTAL passed={passed} failed={failed}");Environment.ExitCode=failed==0?0:1;
sealed class PublicTenant(int id):IPublicTenantResolver{public Task<int> ResolveAsync(CancellationToken ct=default)=>Task.FromResult(id);}
sealed class Notifications:IBookingNotifications{public Task StageAsync(Booking b,BookingNotificationEvent e,DateTime? reminderDueAt=null,CancellationToken cancellationToken=default,long? eventInstance=null,string? detail=null)=>Task.CompletedTask;}
sealed class Host:IHostEnvironment{public string EnvironmentName{get;set;}="Development";public string ApplicationName{get;set;}="FilesFixture";public string ContentRootPath{get;set;}="";public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();}

sealed class Tenant:ICurrentTenantContext{public Task<VerifiedTenant> RequireAsync(CancellationToken ct=default)=>Task.FromResult(new VerifiedTenant(1,1,1,"Admin","files"));public Task DemandAsync(params string[] permissions)=>Task.CompletedTask;}

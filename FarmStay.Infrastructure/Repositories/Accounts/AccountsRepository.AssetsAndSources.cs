using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using FarmStay.Domain.Accounts;
using FarmStay.Application.Accounts;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 async Task<FixedAsset> Acquire(AccountScope s,AssetInput r,CancellationToken ct) {
  AccountingRules.Money(r.Cost);await Master(s,r.CategoryId,"AssetCategory",ct);await Master(s,r.AssetTypeId,"AssetType",ct);if(r.VendorId.HasValue)await Owned<Vendor>(s,r.VendorId.Value,ct);
  Require(r.InServiceDate>=r.PurchaseDate&&r.PurchaseDate<=DateTime.Today,"Check purchase and in-service dates.");
  AccountingRules.Depreciation(r.Cost,r.ResidualValue,0,r.UsefulLifeMonths,r.DepreciationMethod,r.AnnualRate);Require(r.DepreciationMethod!="WDV"||r.AnnualRate>0,"WDV needs a configured annual rate.");
  var settings=await db.Set<AccountingSettings>().SingleAsync(x=>x.FarmHouseId==s.FarmHouseId,ct);Require(r.Cost>=settings.CapitalisationThreshold,"Purchase is below the configured capitalisation threshold; use an expense voucher.");
  var credit=await Account(s,r.CreditAccountId,ct);Require(credit.GroupCode=="LIABILITY"||credit.Code is "1000" or "1010"||await db.Set<MoneyAccount>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.AccountId==credit.Id&&x.IsActive,ct),"Choose a payable or money account.");
  var v=await Automatic(s,"AssetAcquisition:"+r.Code,r.PurchaseDate,"Journal","Asset acquisition: "+r.Name,[new(){AccountId=(await Code(s,"1500",ct)).Id,Debit=r.Cost,VendorId=r.VendorId},new(){AccountId=credit.Id,Credit=r.Cost,VendorId=r.VendorId}],ct);
  var a=new FixedAsset{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,Code=r.Code,Name=r.Name,CategoryId=r.CategoryId,AssetTypeId=r.AssetTypeId,VendorId=r.VendorId,SerialNumber=r.SerialNumber,PurchaseDate=r.PurchaseDate,InServiceDate=r.InServiceDate,Cost=r.Cost,ResidualValue=r.ResidualValue,UsefulLifeMonths=r.UsefulLifeMonths,DepreciationMethod=r.DepreciationMethod,AnnualRate=r.AnnualRate,Location=r.Location,WarrantyEnd=r.WarrantyEnd,AcquisitionVoucherId=v.Id};db.Add(a);return a;
 }
 public async Task<object> AssetActionAsync(AccountScope s,long id,string action,ActionInput r,CancellationToken ct) {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @lock int; EXEC @lock=sys.sp_getapplock @Resource={$"Accounts:Asset:{s.FarmHouseId}:{id}"},@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000; IF @lock<0 THROW 51030,'Asset action is busy.',1;",ct);
  var a=await Owned<FixedAsset>(s,id,ct);
  Require(action is "preview" or "depreciation" or "maintenance" or "transfer" or "disposal","Unknown asset action.");
  Require(r.RequestId!=Guid.Empty,"Use a valid action request key.");
  var reason=r.Reason?.Trim()??"";
  var requestHash=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new {Date=r.Date.Date.Ticks,Amount=r.Amount.ToString("G29",CultureInfo.InvariantCulture),r.AccountId,Reason=reason})));
  var prefix=$"Asset:{id}:{action}:"+(r.RequestId?.ToString("N")??Convert.ToHexString(r.Version??[]))+":";
  var key=action=="depreciation"?$"Asset:{id}:depreciation:{r.Date:yyyyMM}":prefix+requestHash;
  if(action!="preview") {
   var prior=await db.Set<AssetEvent>().SingleOrDefaultAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.AssetId==id&&(action=="depreciation"?x.EventKey==key:x.EventKey.StartsWith(prefix)),ct);
   if(prior!=null){Require(prior.EventKey==key&&prior.EventDate.Date==r.Date.Date&&prior.Notes==reason,"This action request key was used for different details.",409);return prior;}
  }
  Require(a.Status=="Active","Asset is disposed.");
  var posted=await db.Set<AssetEvent>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.AssetId==id&&x.Kind=="Depreciation").SumAsync(x=>x.Amount,ct);
  var amount=AccountingRules.Depreciation(a.Cost,a.ResidualValue,posted,a.UsefulLifeMonths,a.DepreciationMethod,a.AnnualRate);
  if(action=="preview")return new{assetId=id,cost=a.Cost,accumulated=posted,netBookValue=a.Cost-posted,nextMonthlyAmount=amount};
  Version(a,r.Version);Require(!string.IsNullOrWhiteSpace(r.Reason),"A reason is required.");Require(r.Date>=a.InServiceDate&&r.Date<=DateTime.Today,"Action date must be between in-service date and today.");
  long? voucher=null;
  if(action=="depreciation") {
   var last=await db.Set<AssetEvent>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.AssetId==id&&x.Kind=="Depreciation").MaxAsync(x=>(DateTime?)x.EventDate,ct);
   var next=last?.AddDays(1)??new DateTime(a.InServiceDate.Year,a.InServiceDate.Month,1);var end=new DateTime(next.Year,next.Month,1).AddMonths(1).AddDays(-1);
   Require(r.Date.Date==end&&amount>0,"Post depreciation chronologically at each month end, up to today's date.");key=$"Asset:{id}:depreciation:{end:yyyyMM}";
   var v=await Automatic(s,key,r.Date,"Depreciation",r.Reason,[new(){AccountId=(await Code(s,"6090",ct)).Id,Debit=amount},new(){AccountId=(await Code(s,"1590",ct)).Id,Credit=amount}],ct);voucher=v.Id;
  } else if(action=="maintenance") {
   AccountingRules.Money(r.Amount);Require(r.AccountId.HasValue,"Choose payment account.");await MoneyLedger(s,r.AccountId!.Value,ct);amount=r.Amount;
   voucher=(await Automatic(s,key,r.Date,"Expense",r.Reason,[new(){AccountId=(await Code(s,"6040",ct)).Id,Debit=amount},new(){AccountId=r.AccountId.Value,Credit=amount}],ct)).Id;
  } else if(action=="transfer") {a.Location=r.Reason;amount=0;}
  else if(action=="disposal") {
   AccountingRules.Money(r.Amount,true);Require(r.AccountId.HasValue,"Choose receiving cash/bank account.");await MoneyLedger(s,r.AccountId!.Value,ct);
   var lines=new List<LineInput>{new(){AccountId=(await Code(s,"1500",ct)).Id,Credit=a.Cost}};
   if(r.Amount>0)lines.Add(new(){AccountId=r.AccountId.Value,Debit=r.Amount});if(posted>0)lines.Add(new(){AccountId=(await Code(s,"1590",ct)).Id,Debit=posted});
   var difference=a.Cost-posted-r.Amount;
   if(difference>0)lines.Add(new(){AccountId=(await Code(s,"6130",ct)).Id,Debit=difference});else if(difference<0)lines.Add(new(){AccountId=(await Code(s,"4020",ct)).Id,Credit=-difference});
   voucher=(await Automatic(s,key,r.Date,"Journal","Asset disposal: "+r.Reason,lines,ct)).Id;a.Status="Disposed";a.DisposalVoucherId=voucher;amount=r.Amount;
  } else throw new AccountsException(400,"Unknown asset action.");
  a.ModifiedBy=s.UserId;a.ModifiedDate=DateTime.Now;var ev=new AssetEvent{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,AssetId=id,Kind=action=="depreciation"?"Depreciation":action,EventDate=r.Date,Amount=amount,Notes=reason,VoucherId=voucher,EventKey=key};db.Add(ev);Audit(s,action,"Asset",id,r.Reason);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ev;
 }
 public async Task ProcessSourcesAsync(CancellationToken ct)
 {
  if((await db.Database.GetPendingMigrationsAsync(ct)).Any()){logger.LogInformation("Accounts source worker is waiting for migrations.");return;}
  var tenants=await db.FarmHouses.AsNoTracking().Where(x=>x.IsActive&&!x.IsDeleted&&!db.farmHouseModules.Any(f=>f.FarmHouseId==x.FarmHouseId&&(!f.IsEnabled||f.IsDeleted)&&db.Modules.Any(m=>m.ModuleId==f.ModuleId&&m.ModuleName=="Accounts"))).Select(x=>x.FarmHouseId).ToListAsync(ct);
  foreach(var tenant in tenants){
   try{await InitializeAsync(tenant,0,ct);}catch(Exception ex)when(ex is not OperationCanceledException){logger.LogError(ex,"Accounts initialization failed for {Tenant}",tenant);db.ChangeTracker.Clear();continue;}
   var scope=new AccountScope(tenant,0,"accounting-worker",new HashSet<string>());
   var payments=await db.PaymentTransactions.AsNoTracking().Where(p=>p.FarmHouseId==tenant&&!p.IsDelete&&p.PaymentStatus=="Success"&&!db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="Payment:"+p.PaymentTransactionId)&&!db.Set<AccountingSourceReview>().Any(i=>i.FarmHouseId==tenant&&i.SourceKey=="Payment:"+p.PaymentTransactionId&&(i.Status=="ReviewRequired"||i.Status=="Resolved"))).OrderBy(p=>p.PaymentTransactionId).Take(100).ToListAsync(ct);
   foreach(var p in payments){var key="Payment:"+p.PaymentTransactionId;
    try{
     await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
     await LockSource($"Accounts:Source:{tenant}:{key}",ct);
     var booking=await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b=>b.BookingId==p.BookingId&&b.FarmHouseId==tenant&&!b.IsDelete,ct);Require(booking!=null,"Payment/booking tenant mismatch.");
     Require(p.PaymentType is "Advance" or "Checkout" or "Refund","Unsupported successful payment type. Review its accounting classification.");
     Require(p.CurrencyCode=="INR","Only INR source payments are supported.");
     if(p.Amount==0&&p.PaymentType is "Advance" or "Checkout"){
      await SourceState(scope,key,"Payment",p.PaymentTransactionId,"Resolved","ZERO_NO_MOVEMENT","Legacy zero-amount payment has no money movement; no voucher was created.",ct);await tx.CommitAsync(ct);continue;
     }
     AccountingRules.Money(p.Amount);
     var money=await Settlement(scope,p,ct);LedgerAccount offset;
     if(p.PaymentType=="Refund"){
      Require(p.OriginalPaymentTransactionId.HasValue&&p.MoneyAccountId.HasValue&&p.RefundAccountingKind is "Advance" or "Earned","Refund allocation and actual Money Account require review.");
      var original=await db.PaymentTransactions.AsNoTracking().SingleOrDefaultAsync(x=>x.PaymentTransactionId==p.OriginalPaymentTransactionId&&x.FarmHouseId==tenant&&x.BookingId==p.BookingId&&!x.IsDelete&&x.PaymentStatus=="Success",ct);
      Require(original!=null&&original.PaymentType is "Advance" or "Checkout"&&original.CurrencyCode==p.CurrencyCode,"Refund original payment is invalid.");
      Require(await db.PaymentTransactions.Where(x=>x.FarmHouseId==tenant&&x.OriginalPaymentTransactionId==p.OriginalPaymentTransactionId&&x.PaymentType=="Refund"&&x.PaymentStatus=="Success"&&!x.IsDelete).SumAsync(x=>x.Amount,ct)<=original!.Amount,"Refund exceeds original payment.");
      if(!await db.Set<Voucher>().AnyAsync(v=>v.FarmHouseId==tenant&&v.SourceKey=="Payment:"+original.PaymentTransactionId,ct))continue;
      if(p.RefundAccountingKind=="Earned"&&!await db.Set<Voucher>().AnyAsync(v=>v.FarmHouseId==tenant&&v.SourceKey=="BookingRevenue:"+p.BookingId,ct))continue;
      offset=await Code(scope,p.RefundAccountingKind=="Advance"?"2100":"4000",ct);
      await Automatic(scope,key,(p.PaidAt??p.VerifiedAt??p.CreatedDate).Date,"Payment","Actual refund "+booking!.BookingReference,[new(){AccountId=offset.Id,Debit=p.Amount,CustomerId=booking.CustomerUserId},new(){AccountId=money.Id,Credit=p.Amount,CustomerId=booking.CustomerUserId}],ct,"Booking:"+p.BookingId);
     }else{
      offset=await Code(scope,p.PaymentType=="Advance"?"2100":"1100",ct);
      await Automatic(scope,key,(p.PaidAt??p.VerifiedAt??p.CreatedDate).Date,"Receipt","Booking collection "+booking!.BookingReference,[new(){AccountId=money.Id,Debit=p.Amount,CustomerId=booking.CustomerUserId},new(){AccountId=offset.Id,Credit=p.Amount,CustomerId=booking.CustomerUserId}],ct,"Booking:"+p.BookingId);
     }
     await ResolveSource(scope,key,ct);await tx.CommitAsync(ct);
    }catch(Exception ex)when(ex is AccountsException or ArgumentException){db.ChangeTracker.Clear();await SourceState(scope,key,"Payment",p.PaymentTransactionId,"ReviewRequired","PAYMENT_REVIEW",ex.Message,ct);}
    catch(Exception ex)when(ex is not OperationCanceledException){logger.LogError(ex,"Transient source payment failure {Payment} in {Tenant}; retry pending",p.PaymentTransactionId,tenant);}finally{db.ChangeTracker.Clear();}
   }
   var fulfilled=await db.Bookings.AsNoTracking().Where(b=>b.FarmHouseId==tenant&&!b.IsDelete&&b.Status=="CheckedOut"&&!db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="BookingRevenue:"+b.BookingId)&&!db.Set<AccountingSourceReview>().Any(i=>i.FarmHouseId==tenant&&i.SourceKey=="BookingRevenue:"+b.BookingId&&(i.Status=="ReviewRequired"||i.Status=="Resolved"))).OrderBy(b=>b.BookingId).Take(100).ToListAsync(ct);
   foreach(var b in fulfilled){var key="BookingRevenue:"+b.BookingId;
    try{
     await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);await LockSource($"Accounts:Source:{tenant}:{key}",ct);
     var facts=await db.PaymentTransactions.AsNoTracking().Where(p=>p.FarmHouseId==tenant&&p.BookingId==b.BookingId&&!p.IsDelete&&p.PaymentStatus=="Success").ToListAsync(ct);
     var required=facts.Where(p=>p.Amount>0&&(p.PaymentType is "Advance" or "Checkout"||p.PaymentType=="Refund"&&p.RefundAccountingKind=="Advance")).ToList();
     if(required.Any(p=>!db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="Payment:"+p.PaymentTransactionId)))continue;
     var advance=facts.Where(p=>p.PaymentType=="Advance").Sum(p=>p.Amount)-facts.Where(p=>p.PaymentType=="Refund"&&p.RefundAccountingKind=="Advance").Sum(p=>p.Amount);
     var discount=await db.CheckoutDiscounts.Where(d=>d.FarmHouseId==tenant&&d.BookingId==b.BookingId&&!d.IsDelete).SumAsync(d=>d.Amount,ct);var net=b.TotalAmount-discount;
     Require(net>=advance&&advance>=0&&net>=0,"Invalid fulfilled pricing, advance or discount.");
     if(net==0){await SourceState(scope,key,"Booking",b.BookingId,"Resolved","ZERO_REVENUE","Fully discounted fulfilment: zero earned revenue and no fake money voucher.",ct);await tx.CommitAsync(ct);continue;}
     var lines=new List<LineInput>{new(){AccountId=(await Code(scope,"4000",ct)).Id,Credit=net,CustomerId=b.CustomerUserId}};
     if(advance>0)lines.Add(new(){AccountId=(await Code(scope,"2100",ct)).Id,Debit=advance,CustomerId=b.CustomerUserId});if(net>advance)lines.Add(new(){AccountId=(await Code(scope,"1100",ct)).Id,Debit=net-advance,CustomerId=b.CustomerUserId});
     await Automatic(scope,key,(b.ActualCheckOutAt??b.CheckoutPaidAt??b.CheckOutDate).Date,"Journal","Booking fulfilled "+b.BookingReference,lines,ct,"Booking:"+b.BookingId);await ResolveSource(scope,key,ct);await tx.CommitAsync(ct);
    }catch(Exception ex)when(ex is AccountsException or ArgumentException){db.ChangeTracker.Clear();await SourceState(scope,key,"Booking",b.BookingId,"ReviewRequired","FULFILMENT_REVIEW",ex.Message,ct);}
    catch(Exception ex)when(ex is not OperationCanceledException){logger.LogError(ex,"Transient recognition failure for booking {Booking} in {Tenant}",b.BookingId,tenant);}finally{db.ChangeTracker.Clear();}
   }
   var schedules=await db.Set<RecurringExpense>().AsNoTracking().Where(r=>r.FarmHouseId==tenant&&r.IsActive&&r.NextDate<=DateTime.Today&&(r.EndDate==null||r.NextDate<=r.EndDate)&&!db.Set<AccountingSourceReview>().Any(i=>i.FarmHouseId==tenant&&i.SourceKey=="Recurring:"+r.Id&&i.Status=="ReviewRequired")).OrderBy(r=>r.NextDate).Take(50).Select(r=>r.Id).ToListAsync(ct);
   foreach(var id in schedules){var reviewKey="Recurring:"+id;
    try{await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);await LockSource($"Accounts:Recurring:{tenant}:{id}",ct);var r=await Owned<RecurringExpense>(scope,id,ct);if(!r.IsActive||r.NextDate>DateTime.Today||r.EndDate.HasValue&&r.NextDate>r.EndDate)continue;
     await Automatic(scope,$"Recurring:{r.Id}:{r.NextDate:yyyyMMdd}",r.NextDate,"Expense",r.Name,[new(){AccountId=r.ExpenseAccountId,Debit=r.Amount,VendorId=r.VendorId,CategoryId=r.CategoryId},new(){AccountId=r.CreditAccountId,Credit=r.Amount,VendorId=r.VendorId}],ct);Require(r.IntervalMonths is >=1 and <=120,"Invalid recurrence interval.");r.NextDate=r.NextDate.AddMonths(r.IntervalMonths);await ResolveSource(scope,reviewKey,ct);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }catch(Exception ex)when(ex is AccountsException or ArgumentException){db.ChangeTracker.Clear();await SourceState(scope,reviewKey,"Recurring",id,"ReviewRequired","RECURRING_REVIEW",ex.Message,ct);}
    catch(Exception ex)when(ex is not OperationCanceledException){logger.LogError(ex,"Transient recurring expense failure {Schedule} in {Tenant}",id,tenant);}finally{db.ChangeTracker.Clear();}
   }
  }
 }
}

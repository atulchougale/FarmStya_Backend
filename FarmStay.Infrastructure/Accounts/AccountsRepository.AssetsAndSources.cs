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
  var a=await Owned<FixedAsset>(s,id,ct);Require(a.Status=="Active","Asset is disposed.");
  var posted=await db.Set<AssetEvent>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.AssetId==id&&x.Kind=="Depreciation").SumAsync(x=>x.Amount,ct);
  var amount=AccountingRules.Depreciation(a.Cost,a.ResidualValue,posted,a.UsefulLifeMonths,a.DepreciationMethod,a.AnnualRate);
  if(action=="preview")return new{assetId=id,cost=a.Cost,accumulated=posted,netBookValue=a.Cost-posted,nextMonthlyAmount=amount};
  Version(a,r.Version);Require(!string.IsNullOrWhiteSpace(r.Reason),"A reason is required.");Require(r.Date>=a.InServiceDate&&r.Date<=DateTime.Today,"Action date must be between in-service date and today.");
  var key=$"Asset:{id}:{action}:{Guid.NewGuid():N}";long? voucher=null;
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
  a.ModifiedBy=s.UserId;a.ModifiedDate=DateTime.Now;var ev=new AssetEvent{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,AssetId=id,Kind=action=="depreciation"?"Depreciation":action,EventDate=r.Date,Amount=amount,Notes=r.Reason,VoucherId=voucher,EventKey=key};db.Add(ev);Audit(s,action,"Asset",id,r.Reason);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ev;
 }
 public async Task ProcessSourcesAsync(CancellationToken ct)
 {
  if((await db.Database.GetPendingMigrationsAsync(ct)).Any()) {logger.LogInformation("Accounts source worker is waiting for database migrations.");return;}
  // Read-only integration: never mutates booking/payment state, sends messages, or replays provider actions.
  var tenants=await db.FarmHouses.AsNoTracking().Where(x=>x.IsActive&&!x.IsDeleted&&!db.farmHouseModules.Any(f=>f.FarmHouseId==x.FarmHouseId&&(!f.IsEnabled||f.IsDeleted)&&db.Modules.Any(m=>m.ModuleId==f.ModuleId&&m.ModuleName=="Accounts"))).Select(x=>x.FarmHouseId).ToListAsync(ct);
  foreach(var tenant in tenants) {
   try {await InitializeAsync(tenant,0,ct);} catch(Exception ex) when(ex is not OperationCanceledException) { logger.LogError(ex,"Accounts initialization failed for tenant {Tenant}",tenant);db.ChangeTracker.Clear();continue; }
   var scope=new AccountScope(tenant,0,"accounting-worker",new HashSet<string>());
   var payments=await db.PaymentTransactions.AsNoTracking().Where(x=>x.FarmHouseId==tenant&&!x.IsDelete&&x.PaymentStatus=="Success"&&!db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="Payment:"+x.PaymentTransactionId)).OrderBy(x=>x.PaymentTransactionId).Take(100).ToListAsync(ct);
   foreach(var p in payments) {
    try {
     await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
     var booking=await db.Bookings.AsNoTracking().SingleOrDefaultAsync(x=>x.BookingId==p.BookingId&&x.FarmHouseId==tenant&&!x.IsDelete,ct);Require(booking!=null,"Payment/booking tenant mismatch.");AccountingRules.Money(p.Amount);Require(p.CurrencyCode=="INR","Only INR sources are supported.");
     var cash=await Code(scope,p.PaymentMethod=="Cash"?"1000":"1010",ct);var offset=await Code(scope,p.PaymentType=="Advance"?"2100":"1100",ct);
     Require(p.PaymentType is "Advance" or "Checkout","Unsupported payment source type.");
     await Automatic(scope,"Payment:"+p.PaymentTransactionId,(p.PaidAt??p.VerifiedAt??p.CreatedDate).Date,"Receipt","Booking payment "+booking!.BookingReference,[new(){AccountId=cash.Id,Debit=p.Amount,CustomerId=booking.CustomerUserId},new(){AccountId=offset.Id,Credit=p.Amount,CustomerId=booking.CustomerUserId}],ct,"Booking:"+booking.BookingId);
     await tx.CommitAsync(ct);
    }catch(Exception ex) when(ex is not OperationCanceledException){logger.LogError(ex,"Accounts source payment {Payment} failed for tenant {Tenant}; will retry",p.PaymentTransactionId,tenant);}finally{db.ChangeTracker.Clear();}
   }
   var fulfilled=await db.Bookings.AsNoTracking().Where(x=>x.FarmHouseId==tenant&&!x.IsDelete&&x.Status=="CheckedOut"&&!db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="BookingRevenue:"+x.BookingId)).OrderBy(x=>x.BookingId).Take(100).ToListAsync(ct);
   foreach(var b in fulfilled) {
    try {
     await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
     var paymentsForBooking=await db.PaymentTransactions.Where(x=>x.FarmHouseId==tenant&&x.BookingId==b.BookingId&&!x.IsDelete&&x.PaymentStatus=="Success").ToListAsync(ct);
     Require(paymentsForBooking.All(x=>db.Set<Voucher>().Any(v=>v.FarmHouseId==tenant&&v.SourceKey=="Payment:"+x.PaymentTransactionId)),"Waiting for source payment postings.");
     var advance=paymentsForBooking.Where(x=>x.PaymentType=="Advance").Sum(x=>x.Amount);var discount=await db.CheckoutDiscounts.Where(x=>x.FarmHouseId==tenant&&x.BookingId==b.BookingId&&!x.IsDelete).SumAsync(x=>x.Amount,ct);var net=b.TotalAmount-discount;
     Require(net>=advance&&net>0,"Invalid source pricing or discount.");
     var lines=new List<LineInput>{new(){AccountId=(await Code(scope,"4000",ct)).Id,Credit=net,CustomerId=b.CustomerUserId}};
     if(advance>0)lines.Add(new(){AccountId=(await Code(scope,"2100",ct)).Id,Debit=advance,CustomerId=b.CustomerUserId});if(net>advance)lines.Add(new(){AccountId=(await Code(scope,"1100",ct)).Id,Debit=net-advance,CustomerId=b.CustomerUserId});
     await Automatic(scope,"BookingRevenue:"+b.BookingId,(b.ActualCheckOutAt??b.CheckoutPaidAt??b.CheckOutDate).Date,"Journal","Booking fulfilled "+b.BookingReference,lines,ct,"Booking:"+b.BookingId);await tx.CommitAsync(ct);
    }catch(Exception ex) when(ex is not OperationCanceledException){logger.LogError(ex,"Accounts recognition for booking {Booking} tenant {Tenant} failed; will retry",b.BookingId,tenant);}finally{db.ChangeTracker.Clear();}
   }
   var schedules=await db.Set<RecurringExpense>().AsNoTracking().Where(x=>x.FarmHouseId==tenant&&x.IsActive&&x.NextDate<=DateTime.Today&&(x.EndDate==null||x.NextDate<=x.EndDate)).OrderBy(x=>x.NextDate).Take(50).Select(x=>x.Id).ToListAsync(ct);
   foreach(var id in schedules) try {
    await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var r=await Owned<RecurringExpense>(scope,id,ct);
    await Automatic(scope,$"Recurring:{r.Id}:{r.NextDate:yyyyMMdd}",r.NextDate,"Expense",r.Name,[new(){AccountId=r.ExpenseAccountId,Debit=r.Amount,VendorId=r.VendorId,CategoryId=r.CategoryId},new(){AccountId=r.CreditAccountId,Credit=r.Amount,VendorId=r.VendorId}],ct);r.NextDate=r.NextDate.AddMonths(r.IntervalMonths);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
   }catch(Exception ex) when(ex is not OperationCanceledException){logger.LogError(ex,"Recurring expense {Schedule} tenant {Tenant} failed; will retry",id,tenant);}finally{db.ChangeTracker.Clear();}
  }
 }
}

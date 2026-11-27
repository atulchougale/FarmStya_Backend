using FarmStay.Domain.Accounts;
using FarmStay.Application.Accounts;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 public async Task<object> GetAsync(AccountScope s,string area,AccountQuery q,CancellationToken ct)
 {
  var t=s.FarmHouseId;
  switch(area) {
   case "access":return new {s.Permissions};
   case "customers":return await Paged(db.UserMemberships.AsNoTracking().Where(x=>x.FarmHouseId==t&&x.IsActive&&!x.IsDeleted&&x.User!.IsActive&&!x.User.IsDeleted&&x.Role!.RoleName=="Customer"&&x.User.FullName.Contains(q.Search)).Select(x=>new {Id=x.UserId,Name=x.User!.FullName}).Distinct().OrderBy(x=>x.Name),q,ct);
   case "settings":return await db.Set<AccountingSettings>().AsNoTracking().SingleAsync(x=>x.FarmHouseId==t,ct);
   case "masters":return await Paged(db.Set<AccountingMaster>().AsNoTracking().Where(x=>(x.FarmHouseId==null||x.FarmHouseId==t)&&(!q.Id.HasValue||x.Id==q.Id)&&(q.Kind==null||x.Kind==q.Kind)&&(x.Name.Contains(q.Search)||x.Code.Contains(q.Search))).OrderBy(x=>x.Kind).ThenBy(x=>x.Code),q,ct);
   case "ledgers":return await Paged(db.Set<LedgerAccount>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&(x.Name.Contains(q.Search)||x.Code.Contains(q.Search))).OrderBy(x=>x.Code),q,ct);
   case "periods":return await Paged(db.Set<AccountingPeriod>().AsNoTracking().Where(x=>x.FarmHouseId==t).OrderByDescending(x=>x.FromDate),q,ct);
   case "financial-years":return await Paged(db.Set<FinancialYear>().AsNoTracking().Where(x=>x.FarmHouseId==t).OrderByDescending(x=>x.FromDate),q,ct);
   case "vendors":return await Paged(db.Set<Vendor>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&(x.Name.Contains(q.Search)||x.Code.Contains(q.Search))).OrderBy(x=>x.Name),q,ct);
   case "money-accounts":return await Paged(db.Set<MoneyAccount>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&(x.Name.Contains(q.Search)||x.Code.Contains(q.Search))).OrderBy(x=>x.Name),q,ct);
   case "assets":return await Paged(db.Set<FixedAsset>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&(x.Name.Contains(q.Search)||x.Code.Contains(q.Search))).OrderBy(x=>x.Code),q,ct);
   case "source-reviews":return await Paged(db.Set<AccountingSourceReview>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(x.SourceKey.Contains(q.Search)||x.Message.Contains(q.Search))&&(q.Status==null||x.Status==q.Status)).OrderByDescending(x=>x.ModifiedDate),q,ct);
   case "asset-events":return await Paged(db.Set<AssetEvent>().AsNoTracking().Where(x=>x.FarmHouseId==t).OrderByDescending(x=>x.EventDate),q,ct);
   case "recurring":return await Paged(db.Set<RecurringExpense>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&x.Name.Contains(q.Search)).OrderBy(x=>x.NextDate),q,ct);
   case "statements":return await Paged(db.Set<BankStatementEntry>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(!q.Id.HasValue||x.Id==q.Id)&&(!q.AccountId.HasValue||x.AccountId==q.AccountId)).OrderByDescending(x=>x.StatementDate),q,ct);
   case "audit":return await Paged(db.Set<AccountingAudit>().AsNoTracking().Where(x=>x.FarmHouseId==t).OrderByDescending(x=>x.Id),q,ct);
   case "overview":return await Dashboard(s,q,ct);
   case "vouchers":case "income":case "expenses":case "payables":case "vendor-payments":
    var v=db.Set<Voucher>().AsNoTracking().Where(x=>x.FarmHouseId==t&&(x.Narration.Contains(q.Search)||x.VoucherNumber.Contains(q.Search))&&(!q.From.HasValue||x.VoucherDate>=q.From)&&(!q.To.HasValue||x.VoucherDate<q.To.Value.AddDays(1))&&(q.Status==null||x.Status==q.Status));
    if(area=="vendor-payments")v=v.Where(x=>x.VoucherType=="VendorPayment");
    if(area=="income")v=v.Where(x=>x.VoucherType=="Income"||x.VoucherType=="Receipt");
    if(area=="expenses")v=v.Where(x=>x.VoucherType=="Expense"||x.VoucherType=="VendorBill"||x.VoucherType=="VendorPayment");
    if(area=="payables") return await Paged(v.Where(x=>x.VoucherType=="VendorBill").OrderByDescending(x=>x.VoucherDate).Select(x=>new {x.Id,x.VoucherNumber,x.VoucherDate,x.DueDate,x.Narration,x.TotalDebit,x.TotalCredit,x.Status,outstanding=x.TotalCredit-(db.Set<Voucher>().Where(p=>p.FarmHouseId==t&&p.Status=="Posted"&&p.SettlesVoucherId==x.Id).Sum(p=>(decimal?)(p.ReversalOfVoucherId.HasValue?-p.TotalDebit:p.TotalDebit))??0)}),q,ct);
    return await Paged(v.OrderByDescending(x=>x.VoucherDate).ThenByDescending(x=>x.Id),q,ct);
   default:throw new AccountsException(404,"Unknown Accounts area.");
  }
 }
 async Task<object> Dashboard(AccountScope s,AccountQuery q,CancellationToken ct) {
  var from=q.From??new DateTime(DateTime.Today.Month<4?DateTime.Today.Year-1:DateTime.Today.Year,4,1);var to=(q.To??DateTime.Today).Date.AddDays(1);
  var lines=db.Set<VoucherLine>().AsNoTracking().Where(x=>x.Voucher.FarmHouseId==s.FarmHouseId&&x.Voucher.Status=="Posted"&&x.Voucher.VoucherDate<to);
  var totals=await lines.GroupBy(x=>x.Account.GroupCode).Select(g=>new {Group=g.Key,Debit=g.Sum(x=>x.Debit),Credit=g.Sum(x=>x.Credit)}).ToListAsync(ct);
  var period=await lines.Where(x=>x.Voucher.VoucherDate>=from).GroupBy(x=>x.Account.GroupCode).Select(g=>new{Group=g.Key,Amount=g.Sum(x=>x.Credit-x.Debit)}).ToListAsync(ct);
  var income=period.Where(x=>x.Group=="INCOME").Sum(x=>x.Amount);var expense=-period.Where(x=>x.Group is "DIRECT_EXP" or "INDIRECT_EXP").Sum(x=>x.Amount);
  var prevFrom=from.AddDays(-(to-from).Days);var previous=await db.Set<VoucherLine>().Where(x=>x.Voucher.FarmHouseId==s.FarmHouseId&&x.Voucher.Status=="Posted"&&x.Voucher.VoucherDate>=prevFrom&&x.Voucher.VoucherDate<from).GroupBy(x=>x.Account.GroupCode).Select(g=>new {Group=g.Key,Amount=g.Sum(x=>x.Credit-x.Debit)}).ToListAsync(ct);
  var balances=await lines.GroupBy(x=>x.Account.Code).Select(g=>new {Code=g.Key,Balance=g.Sum(x=>x.Debit-x.Credit)}).ToListAsync(ct);
  decimal B(string c)=>balances.Where(x=>x.Code==c).Sum(x=>x.Balance);
  var cashCodes=await db.Set<MoneyAccount>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.Kind=="Cash").Join(db.Set<LedgerAccount>(),x=>x.AccountId,x=>x.Id,(m,a)=>a.Code).ToListAsync(ct);
  var bankCodes=await db.Set<MoneyAccount>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.Kind=="Bank").Join(db.Set<LedgerAccount>(),x=>x.AccountId,x=>x.Id,(m,a)=>a.Code).ToListAsync(ct);
  return new {from,to=to.AddDays(-1),income,expenses=expense,netResult=income-expense,cash=B("1000")+balances.Where(x=>cashCodes.Contains(x.Code)).Sum(x=>x.Balance),bank=B("1010")+balances.Where(x=>bankCodes.Contains(x.Code)).Sum(x=>x.Balance),receivables=B("1100"),customerAdvances=-B("2100"),payables=-totals.Where(x=>x.Group=="LIABILITY").Sum(x=>x.Debit-x.Credit)-(-B("2100"))-(-B("2200")),assets=totals.Where(x=>x.Group=="ASSET").Sum(x=>x.Debit-x.Credit),fixedAssets=B("1500")+B("1590"),previousIncome=previous.Where(x=>x.Group=="INCOME").Sum(x=>x.Amount),previousExpenses=-previous.Where(x=>x.Group is "DIRECT_EXP" or "INDIRECT_EXP").Sum(x=>x.Amount)};
 }
 public async Task<object> SaveAsync(AccountScope s,string area,long? id,object input,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  object result;
  switch(area) {
   case "masters": {
    var r=(MasterInput)input; Require(Catalogue.ContainsKey(r.Kind)||r.Kind=="Tax","Invalid master kind."); Require(r.Kind is not ("Group" or "VoucherType" or "DepreciationMethod"),"System controlled master.");
    var m=id.HasValue?await db.Set<AccountingMaster>().SingleOrDefaultAsync(x=>x.Id==id&&x.FarmHouseId==s.FarmHouseId,ct):null;
    Require(!id.HasValue||m!=null,"Master not found.",404);Require(m?.IsSystemDefined!=true,"System records are read-only.",403);Require(m==null||m.Kind==r.Kind,"Master kind cannot change after creation.");
    if(r.Kind=="Tax")Require(await db.Set<AccountingSettings>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.GstEnabled,ct),"Enable GST before defining tax rates.");
    Require(r.Rate==null||r.Rate is >=0 and <=100,"Invalid tax rate."); Require(r.EffectiveFrom<=r.EffectiveTo||r.EffectiveTo==null,"Invalid effective dates.");
    Require(!await db.Set<AccountingMaster>().AnyAsync(x=>x.Id!=(id??0)&&(x.FarmHouseId==null||x.FarmHouseId==s.FarmHouseId)&&x.Kind==r.Kind&&x.Code==r.Code,ct),"Code already exists.");
    if(r.DefaultLedgerCode!=null)Require(await db.Set<LedgerAccount>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.Code==r.DefaultLedgerCode,ct),"Invalid mapped ledger.");
    m??=new(){FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId};m.Kind=r.Kind;m.Code=r.Code.Trim();m.Name=r.Name.Trim();m.Description=r.Description;m.DefaultLedgerCode=r.DefaultLedgerCode;m.Rate=r.Rate;m.EffectiveFrom=r.EffectiveFrom;m.EffectiveTo=r.EffectiveTo;m.IsActive=r.IsActive;m.ModifiedBy=s.UserId;m.ModifiedDate=DateTime.Now;if(!id.HasValue)db.Add(m);result=m;break;
   }
   case "ledgers": {
    var r=(LedgerInput)input; Require(AccountingRules.Groups.Contains(r.GroupCode),"Invalid account group.");
    var a=id.HasValue?await Owned<LedgerAccount>(s,id.Value,ct):new LedgerAccount{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId};Require(!a.IsSystemDefined,"System ledgers are protected.",403);
    if(id.HasValue)Require(!await db.Set<VoucherLine>().AnyAsync(x=>x.AccountId==a.Id,ct)||a.GroupCode==r.GroupCode,"Used account group cannot change.");
    Require(!await db.Set<LedgerAccount>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.Code==r.Code&&x.Id!=(id??0),ct),"Account code already exists.");
    if(r.ParentId.HasValue) {var parent=await Account(s,r.ParentId.Value,ct);Require(parent.GroupCode==r.GroupCode&&parent.Id!=a.Id,"Parent must belong to the same group."); var cursor=parent;while(cursor.ParentId.HasValue){Require(cursor.ParentId!=a.Id,"Account hierarchy cannot contain cycles.");cursor=await Account(s,cursor.ParentId.Value,ct);} }
    if(a.Id>0&&a.GroupCode!=r.GroupCode)Require(!await db.Set<VoucherLine>().AnyAsync(l=>l.AccountId==a.Id&&l.Voucher.Status=="Posted",ct),"Account groups used by posted vouchers cannot be changed.");
    if(a.Id>0&&await db.Set<MoneyAccount>().AnyAsync(m=>m.FarmHouseId==s.FarmHouseId&&m.AccountId==a.Id,ct))Require(r.GroupCode=="ASSET","Money Account ledgers must stay in the asset group.");
    if(a.Id>0&&await db.Set<Vendor>().AnyAsync(v=>v.FarmHouseId==s.FarmHouseId&&v.AccountId==a.Id,ct))Require(r.GroupCode=="LIABILITY","Vendor payable ledgers must stay in the liability group.");
    a.Code=r.Code.Trim();a.Name=r.Name.Trim();a.GroupCode=r.GroupCode;a.ParentId=r.ParentId;a.NormalBalance=r.GroupCode is "ASSET" or "DIRECT_EXP" or "INDIRECT_EXP"?"Debit":"Credit";a.IsActive=r.IsActive;if(!id.HasValue)db.Add(a);result=a;break;
   }
   case "vendors": {
    var r=(VendorInput)input;var v=id.HasValue?await Owned<Vendor>(s,id.Value,ct):new Vendor{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId};
    if(!id.HasValue){var ledger=new LedgerAccount{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,Code="V-"+r.Code,Name=r.Name,GroupCode="LIABILITY",NormalBalance="Credit"};db.Add(ledger);await db.SaveChangesAsync(ct);v.AccountId=ledger.Id;db.Add(v);}
    v.Code=r.Code;v.Name=r.Name;v.Email=r.Email;v.Mobile=r.Mobile;v.Address=r.Address;v.GstNumber=r.GstNumber;v.IsActive=r.IsActive;result=v;break;
   }
   case "money-accounts": {
    var r=(MoneyAccountInput)input;Require(r.Kind is "Bank" or "Cash","Choose Bank or Cash.");
    if(id.HasValue) {var existing=await Owned<MoneyAccount>(s,id.Value,ct);Require(existing.Kind==r.Kind&&existing.Code==r.Code,"Money account code and type cannot be reassigned.");existing.Name=r.Name;existing.AccountNumberLast4=r.AccountNumberLast4;existing.Ifsc=r.Ifsc;existing.IsActive=r.IsActive;var linked=await Owned<LedgerAccount>(s,existing.AccountId,ct);linked.IsActive=r.IsActive;linked.Name=r.Name;result=existing;break;}
    var ledger=new LedgerAccount{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,Code="M-"+r.Code,Name=r.Name,GroupCode="ASSET"};db.Add(ledger);await db.SaveChangesAsync(ct);
    var a=new MoneyAccount{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,Code=r.Code,Name=r.Name,Kind=r.Kind,AccountId=ledger.Id,AccountNumberLast4=r.AccountNumberLast4,Ifsc=r.Ifsc};db.Add(a);result=a;break;
   }
   case "settings": {
    var r=(SettingsInput)input;AccountingRules.Money(r.CapitalisationThreshold,true);Require(!r.GstEnabled||System.Text.RegularExpressions.Regex.IsMatch(r.GstNumber??"","^[0-9]{2}[A-Z0-9]{13}$"),"Enter a valid GST number before enabling GST.");var set=await db.Set<AccountingSettings>().SingleAsync(x=>x.FarmHouseId==s.FarmHouseId,ct);set.GstEnabled=r.GstEnabled;set.GstNumber=r.GstNumber;set.CapitalisationThreshold=r.CapitalisationThreshold;
    foreach(var item in new[]{(r.DefaultCashMoneyAccountId,"Cash"),(r.DefaultBankMoneyAccountId,"Bank")})if(item.Item1.HasValue){var money=await Owned<MoneyAccount>(s,item.Item1.Value,ct);Require(money.IsActive&&money.Kind==item.Item2,"Select an active "+item.Item2+" Money Account.");await MoneyLedger(s,money.AccountId,ct);}
    set.DefaultCashMoneyAccountId=r.DefaultCashMoneyAccountId;set.DefaultBankMoneyAccountId=r.DefaultBankMoneyAccountId;result=set;break;
   }
   case "source-reviews": {
    Require(id.HasValue,"Select a source review.");var r=(ActionInput)input;var issue=await Owned<AccountingSourceReview>(s,id!.Value,ct);Version(issue,r.Version);Require(issue.Status=="ReviewRequired"&&!string.IsNullOrWhiteSpace(r.Reason),"Review the cause and enter a retry reason.");issue.Status="RetryRequested";Audit(s,"RetrySource","SourceReview",issue.Id,r.Reason.Trim());result=issue;break;
   }
   case "periods": {
    Require(id.HasValue,"Select a period.");var r=(ActionInput)input;var p=await Owned<AccountingPeriod>(s,id!.Value,ct);Version(p,r.Version);Require(!string.IsNullOrWhiteSpace(r.Reason),"Period change needs a reason.");p.IsClosed=r.Amount==1;result=p;break;
   }
   case "vouchers":case "income":case "expenses":case "payables":result=await Draft(s,id,(VoucherInput)input,ct);break;
   case "assets":Require(!id.HasValue,"Acquired assets must be changed through asset actions.");result=await Acquire(s,(AssetInput)input,ct);break;
   case "recurring": {
    var r=(RecurringInput)input;AccountingRules.Money(r.Amount);var a=await Account(s,r.ExpenseAccountId,ct);Require(a.GroupCode is "DIRECT_EXP" or "INDIRECT_EXP","Choose an expense ledger.");var credit=await Account(s,r.CreditAccountId,ct);var vendor=r.VendorId.HasValue?await Owned<Vendor>(s,r.VendorId.Value,ct):null;
    var money=credit.Code is "1000" or "1010"||await db.Set<MoneyAccount>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.AccountId==credit.Id&&x.IsActive,ct);
    Require(money||credit.Code=="2000"||vendor is {IsActive:true}&&vendor.AccountId==credit.Id,"Recurring expenses need a cash, bank or selected vendor payable credit.");
    Require(r.IntervalMonths is >=1 and <=120&&r.NextDate.Year>=2000,"Enter a valid recurrence interval and date.");await Master(s,r.CategoryId,"ExpenseCategory",ct);Require(r.NextDate<=r.EndDate||r.EndDate==null,"Invalid recurrence dates.");var e=id.HasValue?await Owned<RecurringExpense>(s,id.Value,ct):new(){FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId};e.Name=r.Name;e.Amount=r.Amount;e.ExpenseAccountId=r.ExpenseAccountId;e.CreditAccountId=r.CreditAccountId;e.VendorId=r.VendorId;e.CategoryId=r.CategoryId;e.IsActive=r.IsActive;e.IntervalMonths=r.IntervalMonths;e.NextDate=r.NextDate;e.EndDate=r.EndDate;if(!id.HasValue)db.Add(e);result=e;break;
   }
   case "statements": {Require(!id.HasValue,"Statement entries are immutable.");var r=(StatementInput)input;await MoneyLedger(s,r.AccountId,ct);AccountingRules.Money(r.Debit,true);AccountingRules.Money(r.Credit,true);Require((r.Debit>0)!=(r.Credit>0),"Enter one positive statement amount.");var entry=new BankStatementEntry{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,AccountId=r.AccountId,StatementDate=r.Date,Reference=r.Reference,Debit=r.Debit,Credit=r.Credit};db.Add(entry);result=entry;break;}
   default:throw new AccountsException(404,"Unknown Accounts operation.");
  }
  if(result is AccountRecord changed && (id.HasValue||area=="settings")){changed.ModifiedBy=s.UserId;changed.ModifiedDate=DateTime.Now;}
  await db.SaveChangesAsync(ct); var entityId=result is AccountRecord rec?rec.Id:result is AccountingMaster master?master.Id:0;
  Audit(s,id.HasValue?"Update":"Create",area,entityId,"Saved "+area);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;
 }
 static void Version(AccountRecord r,byte[]? version) {Require(version!=null&&r.Version.SequenceEqual(version),"This record changed. Refresh and try again.",409);}
 async Task<LedgerAccount> MoneyLedger(AccountScope s,long id,CancellationToken ct) {var a=await Account(s,id,ct);Require(a.GroupCode=="ASSET","Cash/bank accounts must be asset ledgers.");Require(a.Code is "1000" or "1010"||await db.Set<MoneyAccount>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.AccountId==id&&x.IsActive,ct),"Choose an active cash or bank ledger.");return a;}
}

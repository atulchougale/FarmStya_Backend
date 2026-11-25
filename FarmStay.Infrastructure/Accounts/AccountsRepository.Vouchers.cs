using FarmStay.Domain.Accounts;
using FarmStay.Application.Accounts;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 async Task<Voucher> Draft(AccountScope s,long? id,VoucherInput r,CancellationToken ct)
 {
  Require(AccountingRules.VoucherTypes.Contains(r.Type)&&r.Type!="Reversal","Invalid voucher type.");Require(!string.IsNullOrWhiteSpace(r.Narration),"Narration is required.");
  Require(r.Date.Year>=2000&&r.Date.Year<=DateTime.Today.Year+1,"Voucher date is outside the supported range.");
  var v=id.HasValue?await db.Set<Voucher>().Include(x=>x.Lines).SingleOrDefaultAsync(x=>x.Id==id&&x.FarmHouseId==s.FarmHouseId,ct):null;
  Require(!id.HasValue||v!=null,"Voucher not found.",404);
  if(v!=null){Require(v.Status=="Draft","Posted vouchers are immutable.",409);Version(v,r.Version);db.RemoveRange(v.Lines);v.Lines.Clear();}
  var year=await EnsureYear(s.FarmHouseId,s.UserId,r.Date,ct);var period=year.Periods.Single(x=>x.FromDate<=r.Date.Date&&x.ToDate>=r.Date.Date);Require(!year.IsClosed&&!period.IsClosed,"Accounting period is closed.");
  v??=new Voucher{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,VoucherNumber="D-"+Guid.NewGuid().ToString("N")};
  v.VoucherDate=r.Date.Date;v.VoucherType=r.Type;v.Narration=r.Narration.Trim();v.SourceReference=r.Reference;v.DueDate=r.DueDate;v.PaymentModeId=r.PaymentModeId;v.SettlesVoucherId=r.SettlesVoucherId;v.FinancialYearId=year.Id;v.AccountingPeriodId=period.Id;
  v.Lines=r.Lines.Select((l,i)=>new VoucherLine{LineNumber=i+1,AccountId=l.AccountId,Debit=l.Debit,Credit=l.Credit,Description=l.Description,VendorId=l.VendorId,CustomerId=l.CustomerId,CostCentreId=l.CostCentreId,CategoryId=l.CategoryId,TaxId=l.TaxId}).ToList();
  await Validate(s,v,ct);v.TotalDebit=v.Lines.Sum(x=>x.Debit);v.TotalCredit=v.Lines.Sum(x=>x.Credit);
  if(!id.HasValue){db.Add(v);await db.SaveChangesAsync(ct);v.VoucherNumber=$"AV-{year.FromDate.Year}-{v.Id:D6}";}
  v.ModifiedBy=s.UserId;v.ModifiedDate=DateTime.Now;return v;
 }
 async Task Validate(AccountScope s,Voucher v,CancellationToken ct)
 {
  AccountingRules.ValidateLines(v.Lines);
  Require(v.Lines.Select(x=>x.AccountId).Distinct().Count()>1,"Use at least two different ledger accounts.");
  if(v.PaymentModeId.HasValue) await Master(s,v.PaymentModeId,"PaymentMode",ct);
  var accounts=await db.Set<LedgerAccount>().Where(x=>x.FarmHouseId==s.FarmHouseId&&v.Lines.Select(l=>l.AccountId).Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
  foreach(var line in v.Lines) Require(accounts.ContainsKey(line.AccountId),"Account does not belong to this farmhouse.");
  bool Expense(VoucherLine l)=>accounts[l.AccountId].GroupCode is "DIRECT_EXP" or "INDIRECT_EXP" || accounts[l.AccountId].IsTaxAccount;
  if(v.VoucherType=="Income") Require(v.Lines.Where(x=>x.Credit>0).All(x=>accounts[x.AccountId].GroupCode=="INCOME"||accounts[x.AccountId].IsTaxAccount),"Income must credit an income or output-tax ledger.");
  if(v.VoucherType is "Expense" or "VendorBill") Require(v.Lines.Where(x=>x.Debit>0).All(Expense),"Expense and vendor bills must debit expense or input-tax ledgers.");
  if(v.VoucherType=="VendorBill") Require(v.Lines.Where(x=>x.Credit>0).All(x=>accounts[x.AccountId].GroupCode=="LIABILITY"&&x.VendorId.HasValue),"Vendor bills must credit the vendor payable ledger.");
  if(v.VoucherType=="VendorPayment") Require(v.SettlesVoucherId.HasValue,"A vendor payment must reference a vendor bill.");
  if(v.VoucherType=="Transfer") foreach(var line in v.Lines) await MoneyLedger(s,line.AccountId,ct);
  if(v.VoucherType=="OpeningBalance") Require(v.Lines.Any(x=>accounts[x.AccountId].Code is "3000" or "3100"),"Opening balances require an equity balancing line.");
  foreach(var line in v.Lines) {
   await Account(s,line.AccountId,ct);
   if(v.VoucherType=="VendorBill"&&line.Credit>0&&line.VendorId.HasValue) Require((await Owned<Vendor>(s,line.VendorId.Value,ct)).AccountId==line.AccountId,"Vendor bill must use the selected vendor's payable ledger.");
   if(line.VendorId.HasValue){var vendor=await Owned<Vendor>(s,line.VendorId.Value,ct);Require(vendor.IsActive,"Inactive vendor.");}
   if(line.CustomerId.HasValue)Require(await db.UserMemberships.AnyAsync(x=>x.UserId==line.CustomerId&&x.FarmHouseId==s.FarmHouseId&&x.IsActive&&!x.IsDeleted,ct),"Customer does not belong to this farmhouse.");
   await Master(s,line.CostCentreId,"CostCentre",ct);
   if(line.CategoryId.HasValue){var category=await db.Set<AccountingMaster>().SingleOrDefaultAsync(x=>x.Id==line.CategoryId&&(x.FarmHouseId==null||x.FarmHouseId==s.FarmHouseId)&&x.IsActive,ct);Require(category?.Kind is "IncomeCategory" or "ExpenseCategory","Invalid category.");}
   if(line.TaxId.HasValue){var tax=await Master(s,line.TaxId,"Tax",ct);Require(await db.Set<AccountingSettings>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.GstEnabled,ct)&&tax?.EffectiveFrom<=v.VoucherDate&&(tax.EffectiveTo==null||tax.EffectiveTo>=v.VoucherDate),"GST disabled or tax is outside its effective dates.");}
  }
  if(v.SettlesVoucherId.HasValue) {
   var bill=await Owned<Voucher>(s,v.SettlesVoucherId.Value,ct);Require(bill.Status=="Posted"&&bill.VoucherType=="VendorBill"&&v.VoucherType is "VendorPayment" or "Reversal","Select a posted vendor bill.");
   var billLines=await db.Set<VoucherLine>().Where(x=>x.VoucherId==bill.Id&&x.Credit>0).ToListAsync(ct);
   var paid=await db.Set<Voucher>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.Status=="Posted"&&x.SettlesVoucherId==bill.Id&&x.Id!=v.Id).SumAsync(x=>x.ReversalOfVoucherId.HasValue?-x.TotalDebit:x.TotalDebit,ct);
   if(v.VoucherType=="VendorPayment") Require(v.Lines.Sum(x=>x.Debit)<=bill.TotalCredit-paid,"Payment exceeds the outstanding bill.");
   if(v.VoucherType=="VendorPayment") Require(v.Lines.Where(x=>x.Debit>0).All(x=>billLines.Any(b=>b.AccountId==x.AccountId&&b.VendorId==x.VendorId)),"Payment must settle the bill's vendor payable ledger.");
  }
 }
 public async Task<object> VoucherAsync(AccountScope s,long id,CancellationToken ct) {
  var v=await db.Set<Voucher>().AsNoTracking().Include(x=>x.Lines).SingleOrDefaultAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.Id==id,ct)??throw new AccountsException(404,"Voucher not found.");
  var settlements=await db.Set<Voucher>().AsNoTracking().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.SettlesVoucherId==id&&x.Status=="Posted").Select(x=>new{x.Id,x.VoucherNumber,x.VoucherDate,amount=x.ReversalOfVoucherId.HasValue?-x.TotalDebit:x.TotalDebit}).ToListAsync(ct);
  return new {voucher=v,settlements,outstanding=v.VoucherType=="VendorBill"?v.TotalCredit-settlements.Sum(x=>x.amount):(decimal?)null,attachments=await db.Set<AccountingAttachment>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.VoucherId==id).Select(x=>new{x.Id,x.FileName,x.ContentType}).ToListAsync(ct)};
 }
 public async Task<object> PostAsync(AccountScope s,long id,ActionInput r,bool reverse,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var v=await db.Set<Voucher>().Include(x=>x.Lines).SingleOrDefaultAsync(x=>x.Id==id&&x.FarmHouseId==s.FarmHouseId,ct)??throw new AccountsException(404,"Voucher not found.");
  Version(v,r.Version);
  Voucher result;
  if(reverse) {
   Require(v.Status=="Posted"&&v.ReversalOfVoucherId==null,"Only an original posted voucher can be reversed.");Require(!string.IsNullOrWhiteSpace(r.Reason),"A reversal reason is required.");
   Require(v.SourceKey==null,"Source-managed vouchers must be corrected through a linked adjustment to preserve source reconciliation.");
   Require(!await db.Set<Voucher>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.ReversalOfVoucherId==v.Id,ct),"Voucher already reversed.",409);
   Require(!await db.Set<Voucher>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.SettlesVoucherId==v.Id&&x.Status=="Posted",ct),"Reverse linked payments before reversing the bill.");
   result=await Draft(s,null,new VoucherInput{Date=r.Date,Type="Adjustment",Narration="Reversal: "+r.Reason,Reference=v.VoucherNumber,Lines=v.Lines.Select(x=>new LineInput{AccountId=x.AccountId,Debit=x.Credit,Credit=x.Debit,VendorId=x.VendorId,CustomerId=x.CustomerId,CostCentreId=x.CostCentreId,CategoryId=x.CategoryId}).ToList()},ct);
   result.VoucherType="Reversal";result.ReversalOfVoucherId=v.Id;result.SettlesVoucherId=v.SettlesVoucherId;result.SourceKey="Reversal:"+v.Id;
  } else {Require(v.Status=="Draft","Only draft vouchers can be posted.",409);result=v;}
  await Post(s,result,ct);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;
 }
 async Task Post(AccountScope s,Voucher v,CancellationToken ct) {
  var p=await Owned<AccountingPeriod>(s,v.AccountingPeriodId,ct);var y=await Owned<FinancialYear>(s,v.FinancialYearId,ct);Require(!p.IsClosed&&!y.IsClosed&&p.FromDate<=v.VoucherDate&&p.ToDate>=v.VoucherDate,"Accounting period is closed or invalid.");await Validate(s,v,ct);
  v.TotalDebit=v.Lines.Sum(x=>x.Debit);v.TotalCredit=v.Lines.Sum(x=>x.Credit);v.Status="Posted";v.PostedBy=s.UserId;v.PostedDate=DateTime.Now;v.ModifiedBy=s.UserId;v.ModifiedDate=DateTime.Now;Audit(s,"Post","Voucher",v.Id,v.VoucherNumber);
 }
 async Task<Voucher> Automatic(AccountScope s,string source,DateTime date,string type,string narration,List<LineInput> lines,CancellationToken ct,string? reference=null) {
  var existing=await db.Set<Voucher>().SingleOrDefaultAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.SourceKey==source,ct);if(existing!=null)return existing;
  var v=await Draft(s,null,new(){Date=date,Type=type,Narration=narration,Lines=lines},ct);v.SourceKey=source;v.SourceReference=reference;await Post(s,v,ct);await db.SaveChangesAsync(ct);return v;
 }
 public async Task<object> AttachmentAsync(AccountScope s,long voucher,string name,string type,byte[] content,CancellationToken ct) {
  var v=await Owned<Voucher>(s,voucher,ct);Require(v.Status=="Draft","Attachments must be supplied before posting.");Require(content.Length is >0 and <=5242880,"Maximum file size is 5 MB.");
  var pdf=type=="application/pdf"&&content.Length>4&&System.Text.Encoding.ASCII.GetString(content,0,4)=="%PDF";var jpg=type=="image/jpeg"&&content.Length>2&&content[0]==255&&content[1]==216;var png=type=="image/png"&&content.Length>7&&content.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10});Require(pdf||jpg||png,"Only valid PDF, JPG and PNG files are accepted.");Require(await db.Set<AccountingAttachment>().CountAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.VoucherId==voucher,ct)<5,"Maximum five attachments per voucher.");
  var a=new AccountingAttachment{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,VoucherId=voucher,FileName=Path.GetFileName(name),ContentType=type,Content=content};db.Add(a);Audit(s,"Attach","Voucher",voucher,"Attachment uploaded");await db.SaveChangesAsync(ct);return new{a.Id,a.FileName};
 }
 public async Task<AccountsDownload> DownloadAttachmentAsync(AccountScope s,long id,CancellationToken ct) {var a=await Owned<AccountingAttachment>(s,id,ct);return new(a.Content,a.ContentType,a.FileName);}
 public async Task<object> ReconcileAsync(AccountScope s,long id,ActionInput r,CancellationToken ct) {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var statement=await Owned<BankStatementEntry>(s,id,ct);Version(statement,r.Version);Require(statement.VoucherLineId==null&&r.LineId.HasValue,"Select an unmatched statement and ledger line.");
  var l=await db.Set<VoucherLine>().Include(x=>x.Voucher).SingleOrDefaultAsync(x=>x.Id==r.LineId&&x.Voucher.FarmHouseId==s.FarmHouseId&&x.Voucher.Status=="Posted",ct);
  Require(l!=null&&l.AccountId==statement.AccountId&&l.Debit==statement.Debit&&l.Credit==statement.Credit&&l.ReconciledDate==null,"Statement and book amounts/account must match.");
  statement.VoucherLineId=l!.Id;l.ReconciledDate=r.Date;Audit(s,"Reconcile","Statement",id,r.Reason);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return statement;
 }
}

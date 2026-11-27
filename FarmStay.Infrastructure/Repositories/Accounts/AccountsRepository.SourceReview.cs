using FarmStay.Application.Accounts;
using FarmStay.Domain.Accounts;
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 async Task<LedgerAccount> Settlement(AccountScope s,PaymentTransaction payment,CancellationToken ct)
 {
  Require(payment.PaymentMethod is "Cash" or "Online","Payment method has no approved settlement mapping.");
  var settings=await db.Set<AccountingSettings>().AsNoTracking().SingleAsync(x=>x.FarmHouseId==s.FarmHouseId,ct);
  var id=payment.MoneyAccountId??(payment.PaymentMethod=="Cash"?settings.DefaultCashMoneyAccountId:settings.DefaultBankMoneyAccountId);
  Require(id.HasValue,"Configure the farmhouse's explicit default Cash/Bank Money Account before posting this collection.");
  var money=await Owned<MoneyAccount>(s,id!.Value,ct);Require(money.IsActive&&money.Kind==(payment.PaymentMethod=="Cash"?"Cash":"Bank"),"Settlement Money Account is inactive or has the wrong type.");
  return await MoneyLedger(s,money.AccountId,ct);
 }
 async Task SourceState(AccountScope s,string key,string kind,long id,string status,string code,string message,CancellationToken ct)
 {
  var own=db.Database.CurrentTransaction==null;await using var tx=own?await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct):null;
  await LockSource($"Accounts:Review:{s.FarmHouseId}:{key}",ct);
  var review=await db.Set<AccountingSourceReview>().SingleOrDefaultAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.SourceKey==key,ct);
  if(review==null){review=new(){FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,SourceKey=key,SourceKind=kind,SourceId=id};db.Add(review);}
  review.Status=status;review.Code=code;review.Message=message.Length>1000?message[..1000]:message;review.Attempts++;review.ModifiedBy=s.UserId;review.ModifiedDate=DateTime.Now;await db.SaveChangesAsync(ct);Audit(s,status,"SourceReview",review.Id,key+": "+code);await db.SaveChangesAsync(ct);if(tx!=null)await tx.CommitAsync(ct);
 }
 Task ResolveSource(AccountScope s,string key,CancellationToken ct)=>db.Set<AccountingSourceReview>().Where(x=>x.FarmHouseId==s.FarmHouseId&&x.SourceKey==key).ExecuteUpdateAsync(u=>u.SetProperty(x=>x.Status,"Resolved").SetProperty(x=>x.ModifiedDate,DateTime.Now).SetProperty(x=>x.ModifiedBy,s.UserId),ct);
}

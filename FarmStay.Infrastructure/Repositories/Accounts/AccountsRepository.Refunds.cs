using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using FarmStay.Application.Accounts;
using FarmStay.Domain.Accounts;
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 public async Task<object> RefundPaymentsAsync(AccountScope s,string bookingReference,CancellationToken ct)
 {
  Require(!string.IsNullOrWhiteSpace(bookingReference)&&bookingReference.Length<=40,"Enter a valid booking reference.");
  var booking=await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b=>b.FarmHouseId==s.FarmHouseId&&b.BookingReference==bookingReference.Trim()&&!b.IsDelete,ct);
  Require(booking!=null,"Booking not found.",404);
  var facts=await db.PaymentTransactions.AsNoTracking().Where(p=>p.FarmHouseId==s.FarmHouseId&&p.BookingId==booking!.BookingId&&!p.IsDelete&&p.PaymentStatus=="Success").ToListAsync(ct);
  return new {bookingId=booking!.BookingId,booking.BookingReference,booking.FullName,booking.Status,payments=facts.Where(p=>p.PaymentType is "Advance" or "Checkout").Select(p=>new{p.PaymentTransactionId,p.PaymentType,p.PaymentMethod,p.Amount,p.PaidAt,refunded=facts.Where(r=>r.PaymentType=="Refund"&&r.OriginalPaymentTransactionId==p.PaymentTransactionId).Sum(r=>r.Amount)}),refunds=facts.Where(p=>p.PaymentType=="Refund").Select(p=>new{p.PaymentTransactionId,p.OriginalPaymentTransactionId,p.Amount,p.PaidAt,p.RefundReason,p.RefundReference,p.MoneyAccountId})};
 }
 public async Task<object> RefundAsync(AccountScope s,RefundInput r,CancellationToken ct)
 {
  AccountingRules.Money(r.Amount);Require(r.RequestId!=Guid.Empty&&r.ActuallyPaid,"Confirm that the refund has actually been paid and supply its request key.");
  var reason=r.Reason?.Trim()??"";var reference=string.IsNullOrWhiteSpace(r.Reference)?null:r.Reference.Trim();Require(reason.Length is >0 and <=1000&&(reference==null||reference.Length<=150),"Enter a refund reason and valid reference.");
  var hash=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{r.OriginalPaymentTransactionId,r.MoneyAccountId,Amount=r.Amount.ToString("G29",CultureInfo.InvariantCulture),Reason=reason,Reference=reference??""})));
  var key="manual-refund:"+s.FarmHouseId+":"+r.RequestId.ToString("N");
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  await LockSource($"Accounts:Refund:{s.FarmHouseId}:{r.OriginalPaymentTransactionId}",ct);
  var prior=await db.PaymentTransactions.AsNoTracking().SingleOrDefaultAsync(p=>p.FarmHouseId==s.FarmHouseId&&p.IdempotencyKey==key&&!p.IsDelete,ct);
  if(prior!=null){Require(prior.RefundRequestHash==hash,"Refund request key was used for different details.",409);return new{prior.PaymentTransactionId,prior.Amount,replay=true};}
  var original=await db.PaymentTransactions.AsNoTracking().SingleOrDefaultAsync(p=>p.FarmHouseId==s.FarmHouseId&&p.PaymentTransactionId==r.OriginalPaymentTransactionId&&!p.IsDelete&&p.PaymentStatus=="Success",ct);
  Require(original!=null&&original.PaymentType is "Advance" or "Checkout","Select a verified original advance or checkout payment.",404);
  Require(original.CurrencyCode=="INR","Only INR refunds are supported.");
  await LockSource($"FarmStay:Checkout:{original!.BookingId}",ct);
  await LockSource($"FarmStay:AdvanceOrder:{original.BookingId}",ct);
  var booking=await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b=>b.FarmHouseId==s.FarmHouseId&&b.BookingId==original.BookingId&&!b.IsDelete,ct);Require(booking!=null,"Booking not found.",404);
  var facts=await db.PaymentTransactions.AsNoTracking().Where(p=>p.FarmHouseId==s.FarmHouseId&&p.BookingId==original.BookingId&&!p.IsDelete).ToListAsync(ct);
  Require(!facts.Any(p=>p.PaymentType=="Checkout"&&p.PaymentStatus=="Pending"),"Reconcile the pending checkout before recording an advance refund.",409);
  var refunds=facts.Where(p=>p.PaymentType=="Refund"&&p.PaymentStatus=="Success").ToList();
  Require(refunds.All(p=>p.OriginalPaymentTransactionId.HasValue),"Review legacy unallocated refunds before recording another refund.",409);
  Require(r.Amount<=original.Amount-refunds.Where(p=>p.OriginalPaymentTransactionId==original.PaymentTransactionId).Sum(p=>p.Amount),"Refund exceeds the original payment's refundable balance.");
  Require(r.Amount<=facts.Where(p=>p.PaymentStatus=="Success"&&p.PaymentType is "Advance" or "Checkout").Sum(p=>p.Amount)-refunds.Sum(p=>p.Amount),"Refund exceeds the booking's net paid amount.");
  var account=await Owned<MoneyAccount>(s,r.MoneyAccountId,ct);Require(account.IsActive,"Select an active refund Money Account.");await MoneyLedger(s,account.AccountId,ct);
  Require(account.Kind is "Cash" or "Bank","Select a cash or bank Money Account.");Require(account.Kind!="Bank"||!string.IsNullOrWhiteSpace(reference),"A bank refund needs its actual payment reference.");
  var earned=booking!.Status=="CheckedOut"||await db.Set<Voucher>().AnyAsync(v=>v.FarmHouseId==s.FarmHouseId&&v.SourceKey=="BookingRevenue:"+booking.BookingId&&v.Status=="Posted",ct);Require(earned||!facts.Any(p=>p.PaymentType=="Checkout"&&p.PaymentStatus=="Success"&&p.Amount>0),"Checkout status requires reconciliation before refunding.",409);Require(earned||original.PaymentType=="Advance","Unfulfilled checkout payments need review before refunding.",409);
  var refund=new PaymentTransaction{FarmHouseId=s.FarmHouseId,BookingId=original.BookingId,PaymentType="Refund",PaymentMethod=account.Kind=="Cash"?"Cash":"Online",Amount=r.Amount,CurrencyCode=original.CurrencyCode,PaymentStatus="Success",PaymentProvider="AdministrationRefund",IdempotencyKey=key,OriginalPaymentTransactionId=original.PaymentTransactionId,MoneyAccountId=account.Id,RefundAccountingKind=earned?"Earned":"Advance",RefundReason=reason,RefundReference=reference,RefundRequestHash=hash,PaidAt=DateTime.Now,VerifiedAt=DateTime.Now,CreatedBy=s.UserId};db.Add(refund);await db.SaveChangesAsync(ct);Audit(s,"RecordRefund","Payment",refund.PaymentTransactionId,$"Actual {account.Kind} refund for payment {original.PaymentTransactionId}; Money Account {account.Id}");await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
  return new{refund.PaymentTransactionId,refund.Amount,replay=false};
 }
 async Task LockSource(string resource,CancellationToken ct)=>await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @lock int; EXEC @lock=sys.sp_getapplock @Resource={resource},@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000; IF @lock<0 THROW 51030,'Accounting operation is busy.',1;",ct);
}

using System.ComponentModel.DataAnnotations;
namespace FarmStay.Application.Accounts;
public sealed class RefundInput
{
 public Guid RequestId {get;set;}
 public long OriginalPaymentTransactionId {get;set;}
 public long MoneyAccountId {get;set;}
 public decimal Amount {get;set;}
 [MaxLength(1000)] public string Reason {get;set;}="";
 [MaxLength(150)] public string? Reference {get;set;}
 public bool ActuallyPaid {get;set;}
}

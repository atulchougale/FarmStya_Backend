namespace FarmStay.Domain.Accounts;
public sealed class AccountingSourceReview:AccountRecord
{
 public string SourceKey {get;set;}="";
 public string SourceKind {get;set;}="";
 public long SourceId {get;set;}
 public string Status {get;set;}="ReviewRequired";
 public string Code {get;set;}="";
 public string Message {get;set;}="";
 public int Attempts {get;set;}
}

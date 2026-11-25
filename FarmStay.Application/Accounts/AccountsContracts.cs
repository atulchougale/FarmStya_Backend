using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed record AccountScope(int FarmHouseId,int UserId,string CorrelationId,IReadOnlySet<string> Permissions);
public sealed class AccountsException(int status,string message) : Exception(message) { public int Status { get; }=status; }
public sealed class AccountQuery
{
    public int Page { get; set; }=1;
    public int PageSize { get; set; }=25;
    public string Search { get; set; }="";
    public string Sort { get; set; }="date";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? AccountId { get; set; }
    public string? Status { get; set; }
    public string? Kind { get; set; }
    public void Validate() { if(Page<1||Page>100000||PageSize<1||PageSize>100||Search.Length>100||Sort is not ("date" or "name" or "code")) throw new AccountsException(400,"Invalid pagination, search or sort."); if(From>To) throw new AccountsException(400,"From date must not follow To date."); }
}
public sealed record AccountPage<T>(IReadOnlyList<T> Items,int Total,int Page,int PageSize);
public sealed class MasterInput
{
    [Required,MaxLength(40)] public string Kind { get; set; }="";
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [MaxLength(1000)] public string? Description { get; set; }
    public string? DefaultLedgerCode { get; set; }
    public string? GroupCode { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }=true;
}
public sealed class LedgerInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [Required] public string GroupCode { get; set; }="ASSET";
    public long? ParentId { get; set; }
    public bool IsActive { get; set; }=true;
}
public sealed class VoucherInput
{
    public DateTime Date { get; set; }=DateTime.Today;
    [Required] public string Type { get; set; }="Journal";
    [Required,MaxLength(1000)] public string Narration { get; set; }="";
    [MaxLength(150)] public string? Reference { get; set; }
    public DateTime? DueDate { get; set; }
    public long? SettlesVoucherId { get; set; }
    [Required] public List<LineInput> Lines { get; set; }=[];
    public long? PaymentModeId { get; set; }
    public byte[]? Version { get; set; }
}
public sealed class LineInput
{
    public long AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public long? VendorId { get; set; }
    public int? CustomerId { get; set; }
    public long? CostCentreId { get; set; }
    public long? CategoryId { get; set; }
    public long? TaxId { get; set; }
}
public sealed class VendorInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [MaxLength(20)] public string? Mobile { get; set; }
    [EmailAddress,MaxLength(150)] public string? Email { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(20)] public string? GstNumber { get; set; }
    public bool IsActive { get; set; }=true;
}
public sealed class MoneyAccountInput
{
    public bool IsActive { get; set; } = true;
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public string Kind { get; set; }="Bank";
    [RegularExpression("^[0-9]{4}$")] public string? AccountNumberLast4 { get; set; }
    [MaxLength(15)] public string? Ifsc { get; set; }
}
public sealed class SettingsInput
{
    public bool GstEnabled { get; set; }
    [MaxLength(15)] public string? GstNumber { get; set; }
    public decimal CapitalisationThreshold { get; set; }
}
public sealed class AssetInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public long CategoryId { get; set; }
    public long? AssetTypeId { get; set; }
    public long? VendorId { get; set; }
    [MaxLength(100)] public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }=DateTime.Today;
    public DateTime InServiceDate { get; set; }=DateTime.Today;
    public decimal Cost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public string DepreciationMethod { get; set; }="SLM";
    public decimal AnnualRate { get; set; }
    [MaxLength(150)] public string Location { get; set; }="";
    public DateTime? WarrantyEnd { get; set; }
    public long CreditAccountId { get; set; }
}
public sealed class ActionInput
{
    public DateTime Date { get; set; }=DateTime.Today;
    [MaxLength(1000)] public string Reason { get; set; }="";
    public decimal Amount { get; set; }
    public long? AccountId { get; set; }
    public long? LineId { get; set; }
    public byte[]? Version { get; set; }
}
public sealed class RecurringInput
{
    public bool IsActive { get; set; } = true;
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public long ExpenseAccountId { get; set; }
    public long CreditAccountId { get; set; }
    public long? VendorId { get; set; }
    public long? CategoryId { get; set; }
    public decimal Amount { get; set; }
    [Range(1,12)] public int IntervalMonths { get; set; }=1;
    public DateTime NextDate { get; set; }=DateTime.Today;
    public DateTime? EndDate { get; set; }
}
public sealed class StatementInput
{
    public long AccountId { get; set; }
    public DateTime Date { get; set; }=DateTime.Today;
    [Required,MaxLength(150)] public string Reference { get; set; }="";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
public sealed class AccountReportRow
{
    public long Id { get; set; }
    public string Code { get; set; }="";
    public string Name { get; set; }="";
    public string GroupCode { get; set; }="";
    public DateTime Date { get; set; }
    public string Reference { get; set; }="";
    public string Narration { get; set; }="";
    public decimal Opening { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string Bucket { get; set; }="";
}
public sealed record AccountsDownload(byte[] Content,string ContentType,string FileName);
public interface IAccountsScope { Task<AccountScope> RequireAsync(string permission,CancellationToken ct); }
public interface IAccountsRepository
{
    Task InitializeAsync(int tenant,int actor,CancellationToken ct);
    Task<object> GetAsync(AccountScope scope,string area,AccountQuery query,CancellationToken ct);
    Task<object> SaveAsync(AccountScope scope,string area,long? id,object input,CancellationToken ct);
    Task<object> VoucherAsync(AccountScope scope,long id,CancellationToken ct);
    Task<object> PostAsync(AccountScope scope,long id,ActionInput input,bool reverse,CancellationToken ct);
    Task<object> AssetActionAsync(AccountScope scope,long id,string action,ActionInput input,CancellationToken ct);
    Task<IReadOnlyList<AccountReportRow>> ReportAsync(AccountScope scope,string report,AccountQuery query,bool export,CancellationToken ct);
    Task<AccountsDownload> ExportAsync(AccountScope scope,string report,AccountQuery query,string format,CancellationToken ct);
    Task<object> AttachmentAsync(AccountScope scope,long voucher,string name,string type,byte[] content,CancellationToken ct);
    Task<AccountsDownload> DownloadAttachmentAsync(AccountScope scope,long id,CancellationToken ct);
    Task<object> ReconcileAsync(AccountScope scope,long id,ActionInput input,CancellationToken ct);
    Task ProcessSourcesAsync(CancellationToken ct);
}
public interface IAccountsService
{
    Task<object> GetAsync(string area,AccountQuery query,CancellationToken ct);
    Task<object> SaveAsync(string area,long? id,object input,CancellationToken ct);
    Task<object> VoucherAsync(long id,CancellationToken ct);
    Task<object> PostAsync(long id,ActionInput input,bool reverse,CancellationToken ct);
    Task<object> AssetActionAsync(long id,string action,ActionInput input,CancellationToken ct);
    Task<object> ReportAsync(string report,AccountQuery query,CancellationToken ct);
    Task<AccountsDownload> ExportAsync(string report,AccountQuery query,string format,CancellationToken ct);
    Task<object> AttachmentAsync(long voucher,string name,string type,byte[] content,CancellationToken ct);
    Task<AccountsDownload> DownloadAttachmentAsync(long id,CancellationToken ct);
    Task<object> ReconcileAsync(long id,ActionInput input,CancellationToken ct);
}

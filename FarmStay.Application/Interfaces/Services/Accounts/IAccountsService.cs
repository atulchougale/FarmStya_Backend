using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public interface IAccountsService
{
    Task<object> RefundAsync(RefundInput input,CancellationToken ct);
    Task<object> RefundPaymentsAsync(string bookingReference,CancellationToken ct);

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

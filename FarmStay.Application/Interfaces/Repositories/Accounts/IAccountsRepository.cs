using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public interface IAccountsRepository
{
    Task<object> RefundAsync(AccountScope scope, RefundInput input,CancellationToken ct);
    Task<object> RefundPaymentsAsync(AccountScope scope, string bookingReference,CancellationToken ct);

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

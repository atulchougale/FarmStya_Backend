namespace FarmStay.Application.Accounts;
public sealed class AccountsService(IAccountsScope access,IAccountsRepository repository):IAccountsService
{
    public async Task<object> GetAsync(string area,AccountQuery q,CancellationToken ct) { q.Validate(); var s=await access.RequireAsync("Accounts.View",ct); await repository.InitializeAsync(s.FarmHouseId,s.UserId,ct); return await repository.GetAsync(s,area,q,ct); }
    public async Task<object> SaveAsync(string area,long? id,object input,CancellationToken ct) { var p=area switch {"masters" or "ledgers" or "vendors" or "money-accounts" or "settings"=>"ManageMasters", "periods"=>"ManagePeriods", "assets"=>"ManageAssets", _=>id.HasValue?"Edit":"Create"}; var s=await access.RequireAsync("Accounts."+p,ct); if(area=="recurring") await access.RequireAsync("Accounts.Post",ct); await repository.InitializeAsync(s.FarmHouseId,s.UserId,ct); return await repository.SaveAsync(s,area,id,input,ct); }
    public async Task<object> VoucherAsync(long id,CancellationToken ct)=>await repository.VoucherAsync(await access.RequireAsync("Accounts.View",ct),id,ct);
    public async Task<object> PostAsync(long id,ActionInput input,bool reverse,CancellationToken ct)=>await repository.PostAsync(await access.RequireAsync(reverse?"Accounts.Reverse":"Accounts.Post",ct),id,input,reverse,ct);
    public async Task<object> AssetActionAsync(long id,string action,ActionInput input,CancellationToken ct)=>await repository.AssetActionAsync(await access.RequireAsync("Accounts.ManageAssets",ct),id,action,input,ct);
    public async Task<object> ReportAsync(string report,AccountQuery q,CancellationToken ct) { q.Validate(); return await repository.ReportAsync(await access.RequireAsync("Accounts.View",ct),report,q,false,ct); }
    public async Task<AccountsDownload> ExportAsync(string report,AccountQuery q,string format,CancellationToken ct) { q.Validate(); if(format is not ("pdf" or "excel")) throw new AccountsException(400,"Choose PDF or Excel."); return await repository.ExportAsync(await access.RequireAsync("Accounts.Export",ct),report,q,format,ct); }
    public async Task<object> AttachmentAsync(long voucher,string name,string type,byte[] content,CancellationToken ct)=>await repository.AttachmentAsync(await access.RequireAsync("Accounts.Edit",ct),voucher,name,type,content,ct);
    public async Task<AccountsDownload> DownloadAttachmentAsync(long id,CancellationToken ct)=>await repository.DownloadAttachmentAsync(await access.RequireAsync("Accounts.View",ct),id,ct);
    public async Task<object> ReconcileAsync(long id,ActionInput input,CancellationToken ct)=>await repository.ReconcileAsync(await access.RequireAsync("Accounts.Post",ct),id,input,ct);
}

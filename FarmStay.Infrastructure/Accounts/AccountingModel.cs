using FarmStay.Domain.Accounts;
using FarmStay.Application.Accounts;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public static class AccountingModel
{
 public static void Configure(ModelBuilder b)
 {
   Configure<AccountingSettings>(b,"AccountingSettings"); Configure<LedgerAccount>(b,"LedgerAccounts"); Configure<FinancialYear>(b,"AccountingFinancialYears"); Configure<AccountingPeriod>(b,"AccountingPeriods"); Configure<Voucher>(b,"AccountingVouchers"); Configure<Vendor>(b,"AccountingVendors"); Configure<MoneyAccount>(b,"AccountingMoneyAccounts"); Configure<FixedAsset>(b,"AccountingFixedAssets"); Configure<AssetEvent>(b,"AccountingAssetEvents"); Configure<RecurringExpense>(b,"AccountingRecurringExpenses"); Configure<BankStatementEntry>(b,"AccountingBankStatements"); Configure<AccountingAttachment>(b,"AccountingAttachments"); Configure<AccountingAudit>(b,"AccountingAudits");
   b.Entity<AccountingSettings>().HasIndex(x=>x.FarmHouseId).IsUnique();
   b.Entity<AccountingMaster>().ToTable("AccountingMasters"); b.Entity<AccountingMaster>().HasKey(x=>x.Id);
   b.Entity<AccountingMaster>().HasIndex(x=>new{x.FarmHouseId,x.Kind,x.Code}).IsUnique().HasFilter(null);
   b.Entity<AccountingMaster>().HasOne<FarmStay.Domain.Entities.FarmHouse>().WithMany().HasForeignKey(x=>x.FarmHouseId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<LedgerAccount>().HasIndex(x=>new{x.FarmHouseId,x.Code}).IsUnique(); b.Entity<LedgerAccount>().HasOne(x=>x.Parent).WithMany().HasForeignKey(x=>x.ParentId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<FinancialYear>().HasIndex(x=>new{x.FarmHouseId,x.FromDate}).IsUnique();
   b.Entity<AccountingPeriod>().HasOne(x=>x.FinancialYear).WithMany(x=>x.Periods).HasForeignKey(x=>x.FinancialYearId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<AccountingPeriod>().HasIndex(x=>new{x.FarmHouseId,x.FromDate}).IsUnique();
   b.Entity<Voucher>().ToTable("AccountingVouchers", t=>{t.UseSqlOutputClause(false);t.HasCheckConstraint("CK_AccountingVoucher_Balanced", "[TotalDebit] = [TotalCredit] AND [TotalDebit] > 0");});
   b.Entity<Voucher>().HasOne<AccountingMaster>().WithMany().HasForeignKey(x=>x.PaymentModeId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Voucher>().HasOne<FinancialYear>().WithMany().HasForeignKey(x=>x.FinancialYearId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Voucher>().HasOne<AccountingPeriod>().WithMany().HasForeignKey(x=>x.AccountingPeriodId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Voucher>().HasOne<Voucher>().WithMany().HasForeignKey(x=>x.ReversalOfVoucherId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Voucher>().HasOne<Voucher>().WithMany().HasForeignKey(x=>x.SettlesVoucherId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Voucher>().HasIndex(x=>new{x.FarmHouseId,x.VoucherNumber}).IsUnique();
   b.Entity<Voucher>().HasIndex(x=>new{x.FarmHouseId,x.SourceKey}).IsUnique().HasFilter("[SourceKey] IS NOT NULL");
   b.Entity<Voucher>().HasIndex(x=>new{x.FarmHouseId,x.VoucherDate,x.Status});
   b.Entity<VoucherLine>().ToTable("AccountingVoucherLines",t=>{t.UseSqlOutputClause(false);t.HasCheckConstraint("CK_AccountingVoucherLine_OneSide","([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");});
   b.Entity<VoucherLine>().HasKey(x=>x.Id); b.Entity<VoucherLine>().HasOne(x=>x.Voucher).WithMany(x=>x.Lines).HasForeignKey(x=>x.VoucherId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<VoucherLine>().HasOne(x=>x.Account).WithMany().HasForeignKey(x=>x.AccountId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<VoucherLine>().HasIndex(x=>new{x.VoucherId,x.LineNumber}).IsUnique(); b.Entity<VoucherLine>().HasIndex(x=>new{x.AccountId,x.VoucherId});
   b.Entity<VoucherLine>().HasOne<Vendor>().WithMany().HasForeignKey(x=>x.VendorId).OnDelete(DeleteBehavior.Restrict);
   foreach(var p in new[]{nameof(VoucherLine.CostCentreId),nameof(VoucherLine.TaxId),nameof(VoucherLine.CategoryId)}) b.Entity<VoucherLine>().HasOne<AccountingMaster>().WithMany().HasForeignKey(p).OnDelete(DeleteBehavior.Restrict);
   b.Entity<Vendor>().HasIndex(x=>new{x.FarmHouseId,x.Code}).IsUnique(); b.Entity<Vendor>().HasOne<LedgerAccount>().WithMany().HasForeignKey(x=>x.AccountId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<MoneyAccount>().HasIndex(x=>new{x.FarmHouseId,x.Code}).IsUnique(); b.Entity<MoneyAccount>().HasOne<LedgerAccount>().WithMany().HasForeignKey(x=>x.AccountId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<FixedAsset>().HasIndex(x=>new{x.FarmHouseId,x.Code}).IsUnique();
   b.Entity<FixedAsset>().HasOne<AccountingMaster>().WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<FixedAsset>().HasOne<AccountingMaster>().WithMany().HasForeignKey(x=>x.AssetTypeId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<FixedAsset>().HasOne<Voucher>().WithMany().HasForeignKey(x=>x.AcquisitionVoucherId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<AssetEvent>().HasOne<FixedAsset>().WithMany().HasForeignKey(x=>x.AssetId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<AssetEvent>().HasIndex(x=>new{x.FarmHouseId,x.EventKey}).IsUnique();
   b.Entity<AccountingAttachment>().HasOne<Voucher>().WithMany().HasForeignKey(x=>x.VoucherId).OnDelete(DeleteBehavior.Restrict);
   b.Entity<BankStatementEntry>().HasIndex(x=>new{x.FarmHouseId,x.AccountId,x.Reference,x.StatementDate}).IsUnique();
   b.Entity<BankStatementEntry>().HasIndex(x=>x.VoucherLineId).IsUnique().HasFilter("[VoucherLineId] IS NOT NULL");
   b.Entity<AccountReportRow>().HasNoKey().ToView("AccountingReportResult");
   foreach(var e in b.Model.GetEntityTypes().Where(x=>x.ClrType.Namespace==typeof(Voucher).Namespace || x.ClrType==typeof(AccountReportRow)))
    foreach(var p in e.GetProperties()) { if(p.ClrType==typeof(decimal)||p.ClrType==typeof(decimal?)) {p.SetPrecision(18); p.SetScale(2);} if(p.ClrType==typeof(string)) p.SetMaxLength(p.Name is "Narration" or "Description" or "Summary" or "Notes" ? 1000 : p.Name=="Code"?40:200); }
 }
 static void Configure<T>(ModelBuilder b,string name) where T:AccountRecord { b.Entity<T>().HasBaseType((Type?)null); b.Entity<T>().ToTable(name,table=>table.UseSqlOutputClause(false)); b.Entity<T>().HasKey(x=>x.Id); b.Entity<T>().Property(x=>x.Version).IsRowVersion(); b.Entity<T>().HasIndex(x=>x.FarmHouseId); b.Entity<T>().HasOne<FarmStay.Domain.Entities.FarmHouse>().WithMany().HasForeignKey(x=>x.FarmHouseId).OnDelete(DeleteBehavior.Restrict); }
}

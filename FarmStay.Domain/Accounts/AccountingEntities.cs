using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public abstract class AccountRecord
{
    public long Id { get; set; }
    public int FarmHouseId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}
public sealed class AccountingSettings : AccountRecord
{
    public bool GstEnabled { get; set; }
    public string? GstNumber { get; set; }
    public decimal CapitalisationThreshold { get; set; }
    public string RevenuePolicy { get; set; } = "Checkout";
    public string CurrencyCode { get; set; } = "INR";
}
public sealed class AccountingMaster
{
    public long Id { get; set; }
    public int? FarmHouseId { get; set; }
    public string Kind { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? GroupCode { get; set; }
    public string? DefaultLedgerCode { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsSystemDefined { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
public sealed class LedgerAccount : AccountRecord
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string GroupCode { get; set; } = "";
    public long? ParentId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public LedgerAccount? Parent { get; set; }
    public string NormalBalance { get; set; } = "Debit";
    public bool IsSystemDefined { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTaxAccount { get; set; }
}
public sealed class FinancialYear : AccountRecord
{
    public string Name { get; set; } = "";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsClosed { get; set; }
    public List<AccountingPeriod> Periods { get; set; } = [];
}
public sealed class AccountingPeriod : AccountRecord
{
    public long FinancialYearId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public FinancialYear FinancialYear { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsClosed { get; set; }
}
public sealed class Voucher : AccountRecord
{
    public string VoucherNumber { get; set; } = "";
    public string VoucherType { get; set; } = "Journal";
    public DateTime VoucherDate { get; set; }
    public long FinancialYearId { get; set; }
    public long AccountingPeriodId { get; set; }
    public string Status { get; set; } = "Draft";
    public string Narration { get; set; } = "";
    public string CurrencyCode { get; set; } = "INR";
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string? SourceKey { get; set; }
    public string? SourceReference { get; set; }
    public DateTime? DueDate { get; set; }
    public long? PaymentModeId { get; set; }
    public long? ReversalOfVoucherId { get; set; }
    public long? SettlesVoucherId { get; set; }
    public int? PostedBy { get; set; }
    public DateTime? PostedDate { get; set; }
    public List<VoucherLine> Lines { get; set; } = [];
}
public sealed class VoucherLine
{
    public long Id { get; set; }
    public long VoucherId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public Voucher Voucher { get; set; } = null!;
    public int LineNumber { get; set; }
    public long AccountId { get; set; }
    public LedgerAccount Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public long? VendorId { get; set; }
    public int? CustomerId { get; set; }
    public long? CostCentreId { get; set; }
    public long? TaxId { get; set; }
    public long? CategoryId { get; set; }
    public DateTime? ReconciledDate { get; set; }
}
public sealed class Vendor : AccountRecord
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GstNumber { get; set; }
    public long AccountId { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class MoneyAccount : AccountRecord
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Bank";
    public long AccountId { get; set; }
    public string? AccountNumberLast4 { get; set; }
    public string? Ifsc { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class FixedAsset : AccountRecord
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public long CategoryId { get; set; }
    public long? AssetTypeId { get; set; }
    public long? VendorId { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime InServiceDate { get; set; }
    public decimal Cost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public string DepreciationMethod { get; set; } = "SLM";
    public decimal AnnualRate { get; set; }
    public string Location { get; set; } = "";
    public DateTime? WarrantyEnd { get; set; }
    public string Status { get; set; } = "Active";
    public long AcquisitionVoucherId { get; set; }
    public long? DisposalVoucherId { get; set; }
}
public sealed class AssetEvent : AccountRecord
{
    public long AssetId { get; set; }
    public string Kind { get; set; } = "Depreciation";
    public DateTime EventDate { get; set; }
    public decimal Amount { get; set; }
    public string Notes { get; set; } = "";
    public long? VoucherId { get; set; }
    public string EventKey { get; set; } = "";
}
public sealed class RecurringExpense : AccountRecord
{
    public string Name { get; set; } = "";
    public long ExpenseAccountId { get; set; }
    public long CreditAccountId { get; set; }
    public long? VendorId { get; set; }
    public long? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int IntervalMonths { get; set; } = 1;
    public DateTime NextDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class BankStatementEntry : AccountRecord
{
    public long AccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public string Reference { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long? VoucherLineId { get; set; }
}
public sealed class AccountingAttachment : AccountRecord
{
    public long VoucherId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Content { get; set; } = [];
}
public sealed class AccountingAudit : AccountRecord
{
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public long EntityId { get; set; }
    public string CorrelationId { get; set; } = "";
    public string Summary { get; set; } = "";
}
public static class AccountingRules
{
    public static readonly string[] Groups = ["ASSET", "LIABILITY", "EQUITY", "INCOME", "DIRECT_EXP", "INDIRECT_EXP"];
    public static readonly string[] VoucherTypes = ["Journal","Receipt","Payment","Transfer","Income","Expense","VendorBill","VendorPayment","OpeningBalance","Depreciation","Reversal","Adjustment"];
    public static void Money(decimal value, bool allowZero = false)
    {
        if (value < 0 || (!allowZero && value == 0) || value > 999999999999.99m || decimal.Round(value,2) != value)
            throw new ArgumentException("Amounts must be positive, within limits, and have at most two decimal places.");
    }
    public static void ValidateLines(IEnumerable<VoucherLine> input)
    {
        var lines = input.ToList();
        if (lines.Count < 2 || lines.Count > 200) throw new ArgumentException("A voucher needs 2–200 lines.");
        foreach(var l in lines) { Money(l.Debit,true); Money(l.Credit,true); if ((l.Debit>0)==(l.Credit>0)) throw new ArgumentException("Each line must have exactly one positive debit or credit."); }
        if (lines.Sum(l=>l.Debit)!=lines.Sum(l=>l.Credit)) throw new ArgumentException("Voucher debits and credits must balance.");
    }
    public static decimal Depreciation(decimal cost, decimal residual, decimal posted, int lifeMonths, string method, decimal rate)
    {
        var remaining = Math.Max(0,cost-residual-posted);
        if (lifeMonths <= 0 || residual < 0 || residual > cost || rate < 0 || rate > 100 || method is not ("SLM" or "WDV")) throw new ArgumentException("Invalid depreciation policy.");
        return Math.Min(remaining, decimal.Round(method=="SLM" ? (cost-residual)/lifeMonths : (cost-posted)*rate/1200m,2,MidpointRounding.AwayFromZero));
    }
}

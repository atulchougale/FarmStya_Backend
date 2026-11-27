using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

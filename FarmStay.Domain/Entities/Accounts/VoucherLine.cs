using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

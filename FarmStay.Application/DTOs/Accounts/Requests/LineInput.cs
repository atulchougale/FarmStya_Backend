using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
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

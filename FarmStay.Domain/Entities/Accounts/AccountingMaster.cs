using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

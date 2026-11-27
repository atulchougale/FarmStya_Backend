using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

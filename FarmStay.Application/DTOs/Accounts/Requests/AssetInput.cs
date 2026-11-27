using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class AssetInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public long CategoryId { get; set; }
    public long? AssetTypeId { get; set; }
    public long? VendorId { get; set; }
    [MaxLength(100)] public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }=DateTime.Today;
    public DateTime InServiceDate { get; set; }=DateTime.Today;
    public decimal Cost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public string DepreciationMethod { get; set; }="SLM";
    public decimal AnnualRate { get; set; }
    [MaxLength(150)] public string Location { get; set; }="";
    public DateTime? WarrantyEnd { get; set; }
    public long CreditAccountId { get; set; }
}

using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

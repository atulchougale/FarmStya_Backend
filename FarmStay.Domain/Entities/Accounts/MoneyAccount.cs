using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

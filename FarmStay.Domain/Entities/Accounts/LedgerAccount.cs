using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

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

using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class AccountingAudit : AccountRecord
{
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public long EntityId { get; set; }
    public string CorrelationId { get; set; } = "";
    public string Summary { get; set; } = "";
}

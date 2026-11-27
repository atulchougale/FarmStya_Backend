using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class AccountingPeriod : AccountRecord
{
    public long FinancialYearId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public FinancialYear FinancialYear { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsClosed { get; set; }
}

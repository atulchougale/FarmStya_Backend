using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class FinancialYear : AccountRecord
{
    public string Name { get; set; } = "";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsClosed { get; set; }
    public List<AccountingPeriod> Periods { get; set; } = [];
}

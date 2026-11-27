using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class RecurringExpense : AccountRecord
{
    public string Name { get; set; } = "";
    public long ExpenseAccountId { get; set; }
    public long CreditAccountId { get; set; }
    public long? VendorId { get; set; }
    public long? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int IntervalMonths { get; set; } = 1;
    public DateTime NextDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

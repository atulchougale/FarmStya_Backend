using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class RecurringInput
{
    public bool IsActive { get; set; } = true;
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public long ExpenseAccountId { get; set; }
    public long CreditAccountId { get; set; }
    public long? VendorId { get; set; }
    public long? CategoryId { get; set; }
    public decimal Amount { get; set; }
    [Range(1,12)] public int IntervalMonths { get; set; }=1;
    public DateTime NextDate { get; set; }=DateTime.Today;
    public DateTime? EndDate { get; set; }
}

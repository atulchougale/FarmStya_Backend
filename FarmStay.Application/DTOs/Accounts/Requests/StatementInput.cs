using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class StatementInput
{
    public long AccountId { get; set; }
    public DateTime Date { get; set; }=DateTime.Today;
    [Required,MaxLength(150)] public string Reference { get; set; }="";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

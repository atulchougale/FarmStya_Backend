using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class MoneyAccountInput
{
    public bool IsActive { get; set; } = true;
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    public string Kind { get; set; }="Bank";
    [RegularExpression("^[0-9]{4}$")] public string? AccountNumberLast4 { get; set; }
    [MaxLength(15)] public string? Ifsc { get; set; }
}

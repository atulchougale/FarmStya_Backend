using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class ActionInput
{
    public Guid? RequestId { get; set; }
    public DateTime Date { get; set; }=DateTime.Today;
    [MaxLength(1000)] public string Reason { get; set; }="";
    public decimal Amount { get; set; }
    public long? AccountId { get; set; }
    public long? LineId { get; set; }
    public byte[]? Version { get; set; }
}

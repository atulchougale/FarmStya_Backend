using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class VoucherInput
{
    public DateTime Date { get; set; }=DateTime.Today;
    [Required] public string Type { get; set; }="Journal";
    [Required,MaxLength(1000)] public string Narration { get; set; }="";
    [MaxLength(150)] public string? Reference { get; set; }
    public DateTime? DueDate { get; set; }
    public long? SettlesVoucherId { get; set; }
    [Required] public List<LineInput> Lines { get; set; }=[];
    public long? PaymentModeId { get; set; }
    public byte[]? Version { get; set; }
}

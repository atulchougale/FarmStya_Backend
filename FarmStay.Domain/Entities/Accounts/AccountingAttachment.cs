using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class AccountingAttachment : AccountRecord
{
    public long VoucherId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Content { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class BankStatementEntry : AccountRecord
{
    public long AccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public string Reference { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long? VoucherLineId { get; set; }
}

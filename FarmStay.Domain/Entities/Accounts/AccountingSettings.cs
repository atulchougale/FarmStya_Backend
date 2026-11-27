using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public sealed class AccountingSettings : AccountRecord
{
    public int InitializationVersion { get; set; }
    public long? DefaultCashMoneyAccountId { get; set; }
    public long? DefaultBankMoneyAccountId { get; set; }
    public bool GstEnabled { get; set; }
    public string? GstNumber { get; set; }
    public decimal CapitalisationThreshold { get; set; }
    public string RevenuePolicy { get; set; } = "Checkout";
    public string CurrencyCode { get; set; } = "INR";
}

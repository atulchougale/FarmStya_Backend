using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class SettingsInput
{
    public long? DefaultCashMoneyAccountId { get; set; }
    public long? DefaultBankMoneyAccountId { get; set; }
    public bool GstEnabled { get; set; }
    [MaxLength(15)] public string? GstNumber { get; set; }
    public decimal CapitalisationThreshold { get; set; }
}

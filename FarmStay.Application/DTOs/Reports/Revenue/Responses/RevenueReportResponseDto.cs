using System.Text.Json.Serialization;

namespace FarmStay.Application.DTOs.Reports.Revenue.Responses;
public sealed class RevenueReportResponseDto
{
    [JsonPropertyName("PaymentTransactionId")]
    public long PaymentTransactionId { get; set; }
    [JsonPropertyName("BookingReference")]
    public string? BookingReference { get; set; }
    [JsonPropertyName("FullName")]
    public string? FullName { get; set; }
    [JsonPropertyName("ReceivedAt")]
    public DateTime ReceivedAt { get; set; }
    [JsonPropertyName("PaymentType")]
    public string? PaymentType { get; set; }
    [JsonPropertyName("PaymentMethod")]
    public string? PaymentMethod { get; set; }
    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("ProviderPaymentId")]
    public string? ProviderPaymentId { get; set; }
}

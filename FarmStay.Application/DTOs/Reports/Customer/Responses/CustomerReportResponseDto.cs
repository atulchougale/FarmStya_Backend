using System.Text.Json.Serialization;

namespace FarmStay.Application.DTOs.Reports.Customer.Responses;
public sealed class CustomerReportResponseDto
{
    [JsonPropertyName("CustomerUserId")]
    public int CustomerUserId { get; set; }
    [JsonPropertyName("FullName")]
    public string? FullName { get; set; }
    [JsonPropertyName("MobileNumber")]
    public string? MobileNumber { get; set; }
    [JsonPropertyName("Email")]
    public string? Email { get; set; }
    [JsonPropertyName("RegisteredAt")]
    public DateTime RegisteredAt { get; set; }
    [JsonPropertyName("BookingCount")]
    public long BookingCount { get; set; }
    [JsonPropertyName("LastBookingAt")]
    public DateTime LastBookingAt { get; set; }
    [JsonPropertyName("BookingValue")]
    public decimal BookingValue { get; set; }
}

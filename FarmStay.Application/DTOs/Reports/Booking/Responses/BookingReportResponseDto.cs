using System.Text.Json.Serialization;

namespace FarmStay.Application.DTOs.Reports.Booking.Responses;
public sealed class BookingReportResponseDto
{
    [JsonPropertyName("BookingReference")]
    public string? BookingReference { get; set; }
    [JsonPropertyName("FullName")]
    public string? FullName { get; set; }
    [JsonPropertyName("MobileNumber")]
    public string? MobileNumber { get; set; }
    [JsonPropertyName("CreatedDate")]
    public DateTime CreatedDate { get; set; }
    [JsonPropertyName("CheckInDate")]
    public DateTime CheckInDate { get; set; }
    [JsonPropertyName("CheckOutDate")]
    public DateTime CheckOutDate { get; set; }
    [JsonPropertyName("BookingType")]
    public string? BookingType { get; set; }
    [JsonPropertyName("BookingSource")]
    public string? BookingSource { get; set; }
    [JsonPropertyName("PersonCount")]
    public int PersonCount { get; set; }
    [JsonPropertyName("Status")]
    public string? Status { get; set; }
    [JsonPropertyName("TotalAmount")]
    public decimal TotalAmount { get; set; }
}

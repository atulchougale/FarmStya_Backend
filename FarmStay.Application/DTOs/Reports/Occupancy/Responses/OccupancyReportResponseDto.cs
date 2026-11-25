using System.Text.Json.Serialization;

namespace FarmStay.Application.DTOs.Reports.Occupancy.Responses;
public sealed class OccupancyReportResponseDto
{
    [JsonPropertyName("StayDate")]
    public DateTime StayDate { get; set; }
    [JsonPropertyName("BookingCount")]
    public long BookingCount { get; set; }
    [JsonPropertyName("Adults")]
    public long Adults { get; set; }
    [JsonPropertyName("Children")]
    public long Children { get; set; }
    [JsonPropertyName("Guests")]
    public long Guests { get; set; }
}

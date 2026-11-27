using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed record ReportQuery(int FarmHouseId, DateTime? FromDate, DateTime? ToDateExclusive,
    string? Search, string BookingType = "all", string Source = "all");

// Constructed by the application reporting facade after JWT, membership and permission checks.

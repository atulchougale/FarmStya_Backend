using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed class ReportTotalResponseDto
{
    public long TotalRecords { get; set; }
}

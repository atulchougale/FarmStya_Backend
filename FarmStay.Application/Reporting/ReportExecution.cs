using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed record ReportQuery(int FarmHouseId, DateTime? FromDate, DateTime? ToDateExclusive,
    string? Search, string BookingType = "all", string Source = "all");

// Constructed by the application reporting facade after JWT, membership and permission checks.
public sealed record ReportScope(int FarmHouseId, int UserId, string FarmHouseName, string Address,
    string Contact, bool GlobalAdministrator, IReadOnlySet<string> Permissions, string CorrelationId);

public interface IReportPage
{
    IReadOnlyList<ReportColumn> Columns { get; }
    long Total { get; }
    int Page { get; }
    int PageSize { get; }
    string PeriodLabel { get; }
    [JsonIgnore] IReadOnlyList<object> UntypedRows { get; }
}

public sealed record ReportPage<T>(IReadOnlyList<ReportColumn> Columns, IReadOnlyList<T> Rows,
    long Total, int Page, int PageSize, string PeriodLabel) : IReportPage where T : class
{
    [JsonIgnore] public IReadOnlyList<object> UntypedRows => Rows;
}

public sealed class ReportTotalResponseDto
{
    public long TotalRecords { get; set; }
}

public sealed record ReportCatalogEntry(string Key, string Title, string Description,
    IReadOnlyList<ReportColumn> Columns, bool CanExport);

public enum ReportError { Unauthorized, Forbidden, NotFound, InvalidRequest, ExportLimit }
public sealed class ReportException(ReportError error, string message) : Exception(message)
{
    public ReportError Error { get; } = error;
}

using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public interface IReportPage
{
    IReadOnlyList<ReportColumn> Columns { get; }
    long Total { get; }
    int Page { get; }
    int PageSize { get; }
    string PeriodLabel { get; }
    [JsonIgnore] IReadOnlyList<object> UntypedRows { get; }
}

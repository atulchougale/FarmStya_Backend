using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed record ReportPage<T>(IReadOnlyList<ReportColumn> Columns, IReadOnlyList<T> Rows,
    long Total, int Page, int PageSize, string PeriodLabel) : IReportPage where T : class
{
    [JsonIgnore] public IReadOnlyList<object> UntypedRows => Rows;
}

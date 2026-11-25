using FarmStay.Application.Reporting;

namespace FarmStay.Application.Interfaces.Repositories.Reports;
public interface IReportRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetRowsAsync(ReportQuery query, int page, int pageSize, CancellationToken ct);
    Task<ReportTotalResponseDto> GetSummaryAsync(ReportQuery query, CancellationToken ct);
}

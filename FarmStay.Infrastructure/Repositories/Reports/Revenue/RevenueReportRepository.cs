using FarmStay.Application.DTOs.Reports.Revenue.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Repositories.Reports.Revenue;
public sealed class RevenueReportRepository(AppDbContext db, ILogger<RevenueReportRepository> logger)
    : ReportRepository(db, logger), IRevenueReportRepository
{
    public Task<IReadOnlyList<RevenueReportResponseDto>> GetRowsAsync(ReportQuery q, int page, int pageSize, CancellationToken ct) =>
        ExecuteAsync<RevenueReportResponseDto>($"""
            EXEC dbo.usp_Revenue_GetReport @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}, @Page={page}, @PageSize={pageSize}
            """, "dbo.usp_Revenue_GetReport", ct);
    public async Task<ReportTotalResponseDto> GetSummaryAsync(ReportQuery q, CancellationToken ct) =>
        (await ExecuteAsync<ReportTotalResponseDto>($"""
            EXEC dbo.usp_Revenue_GetSummary @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}
            """, "dbo.usp_Revenue_GetSummary", ct)).Single();
}

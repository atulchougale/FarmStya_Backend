using FarmStay.Application.DTOs.Reports.Occupancy.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Repositories.Reports.Occupancy;
public sealed class OccupancyReportRepository(AppDbContext db, ILogger<OccupancyReportRepository> logger)
    : ReportRepository(db, logger), IOccupancyReportRepository
{
    public Task<IReadOnlyList<OccupancyReportResponseDto>> GetRowsAsync(ReportQuery q, int page, int pageSize, CancellationToken ct) =>
        ExecuteAsync<OccupancyReportResponseDto>($"""
            EXEC dbo.usp_Occupancy_GetReport @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}, @Page={page}, @PageSize={pageSize}
            """, "dbo.usp_Occupancy_GetReport", ct);
    public async Task<ReportTotalResponseDto> GetSummaryAsync(ReportQuery q, CancellationToken ct) =>
        (await ExecuteAsync<ReportTotalResponseDto>($"""
            EXEC dbo.usp_Occupancy_GetSummary @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}
            """, "dbo.usp_Occupancy_GetSummary", ct)).Single();
}

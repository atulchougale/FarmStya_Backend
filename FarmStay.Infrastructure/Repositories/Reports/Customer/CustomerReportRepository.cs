using FarmStay.Application.DTOs.Reports.Customer.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Repositories.Reports.Customer;
public sealed class CustomerReportRepository(AppDbContext db, ILogger<CustomerReportRepository> logger)
    : ReportRepository(db, logger), ICustomerReportRepository
{
    public Task<IReadOnlyList<CustomerReportResponseDto>> GetRowsAsync(ReportQuery q, int page, int pageSize, CancellationToken ct) =>
        ExecuteAsync<CustomerReportResponseDto>($"""
            EXEC dbo.usp_Customer_GetReport @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}, @Page={page}, @PageSize={pageSize}
            """, "dbo.usp_Customer_GetReport", ct);
    public async Task<ReportTotalResponseDto> GetSummaryAsync(ReportQuery q, CancellationToken ct) =>
        (await ExecuteAsync<ReportTotalResponseDto>($"""
            EXEC dbo.usp_Customer_GetSummary @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}
            """, "dbo.usp_Customer_GetSummary", ct)).Single();
}

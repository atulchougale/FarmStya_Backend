using FarmStay.Application.DTOs.Reports.Booking.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Repositories.Reports.Booking;
public sealed class BookingReportRepository(AppDbContext db, ILogger<BookingReportRepository> logger)
    : ReportRepository(db, logger), IBookingReportRepository
{
    public Task<IReadOnlyList<BookingReportResponseDto>> GetRowsAsync(ReportQuery q, int page, int pageSize, CancellationToken ct) =>
        ExecuteAsync<BookingReportResponseDto>($"""
            EXEC dbo.usp_Booking_GetReport @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}, @BookingType={q.BookingType}, @Source={q.Source}, @Page={page}, @PageSize={pageSize}
            """, "dbo.usp_Booking_GetReport", ct);
    public async Task<ReportTotalResponseDto> GetSummaryAsync(ReportQuery q, CancellationToken ct) =>
        (await ExecuteAsync<ReportTotalResponseDto>($"""
            EXEC dbo.usp_Booking_GetSummary @FarmHouseId={q.FarmHouseId}, @FromDate={q.FromDate}, @ToDateExclusive={q.ToDateExclusive},
                @Search={q.Search}, @BookingType={q.BookingType}, @Source={q.Source}
            """, "dbo.usp_Booking_GetSummary", ct)).Single();
}

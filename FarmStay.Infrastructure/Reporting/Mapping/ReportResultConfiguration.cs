using FarmStay.Application.DTOs.Reports.Booking.Responses;
using FarmStay.Application.DTOs.Reports.Revenue.Responses;
using FarmStay.Application.DTOs.Reports.Customer.Responses;
using FarmStay.Application.DTOs.Reports.Occupancy.Responses;
using FarmStay.Application.Reporting;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Reporting.Mapping;
public static class ReportResultConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        // Read-only logical mappings: FromSql supplies the query; no physical DTO tables/views are created.
        model.Entity<BookingReportResponseDto>().HasNoKey().ToView("BookingReportResult");
        model.Entity<BookingReportResponseDto>().Property(r => r.TotalAmount).HasPrecision(18, 2);
        model.Entity<ReportTotalResponseDto>().HasNoKey().ToView("ReportTotalResult");
        model.Entity<RevenueReportResponseDto>().HasNoKey().ToView("RevenueReportResult");
        model.Entity<RevenueReportResponseDto>().Property(r => r.Amount).HasPrecision(18, 2);
        model.Entity<CustomerReportResponseDto>().HasNoKey().ToView("CustomerReportResult");
        model.Entity<CustomerReportResponseDto>().Property(r => r.BookingValue).HasPrecision(38, 2);
        model.Entity<OccupancyReportResponseDto>().HasNoKey().ToView("OccupancyReportResult");
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardSummaryDto>().HasNoKey().ToView("DashboardSummaryResult");
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardSummaryDto>().Property(r => r.TotalRevenue).HasPrecision(38,2);
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardSummaryDto>().Property(r => r.RevenueThisMonth).HasPrecision(38,2);
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardSummaryDto>().Property(r => r.OccupancyPercent).HasPrecision(5,2);
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardCalendarBookingDto>().HasNoKey().ToView("DashboardCalendarResult");
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardRevenueMonthDto>().HasNoKey().ToView("DashboardRevenueMonthResult");
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardRevenueMonthDto>().Property(r => r.Amount).HasPrecision(38,2);
        model.Entity<FarmStay.Application.DTOs.Administration.DashboardBookingSourceDto>().HasNoKey().ToView("DashboardBookingSourceResult");
    }
}

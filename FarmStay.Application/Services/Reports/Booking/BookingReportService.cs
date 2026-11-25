using FarmStay.Application.DTOs.Reports.Booking.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using FluentValidation;
namespace FarmStay.Application.Services.Reports.Booking;
public sealed class BookingReportService(IBookingReportRepository repository, IReportFileRenderer renderer, IValidator<ReportFilter> validator)
    : ReportModuleService<BookingReportResponseDto>("bookings", repository, renderer, validator), IBookingReportService;

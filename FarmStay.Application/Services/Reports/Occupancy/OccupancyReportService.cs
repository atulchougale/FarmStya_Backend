using FarmStay.Application.DTOs.Reports.Occupancy.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using FluentValidation;
namespace FarmStay.Application.Services.Reports.Occupancy;
public sealed class OccupancyReportService(IOccupancyReportRepository repository, IReportFileRenderer renderer, IValidator<ReportFilter> validator)
    : ReportModuleService<OccupancyReportResponseDto>("occupancy", repository, renderer, validator), IOccupancyReportService;

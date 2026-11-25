using FarmStay.Application.DTOs.Reports.Revenue.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using FluentValidation;
namespace FarmStay.Application.Services.Reports.Revenue;
public sealed class RevenueReportService(IRevenueReportRepository repository, IReportFileRenderer renderer, IValidator<ReportFilter> validator)
    : ReportModuleService<RevenueReportResponseDto>("revenue", repository, renderer, validator), IRevenueReportService;

using FarmStay.Application.DTOs.Reports.Customer.Responses;
using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using FluentValidation;
namespace FarmStay.Application.Services.Reports.Customer;
public sealed class CustomerReportService(ICustomerReportRepository repository, IReportFileRenderer renderer, IValidator<ReportFilter> validator)
    : ReportModuleService<CustomerReportResponseDto>("customers", repository, renderer, validator), ICustomerReportService;

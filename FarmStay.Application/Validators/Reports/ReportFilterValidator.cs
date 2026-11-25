using FarmStay.Application.Reporting;
using FluentValidation;

namespace FarmStay.Application.Validators.Reports;
public sealed class ReportFilterValidator : AbstractValidator<ReportFilter>
{
    public ReportFilterValidator()
    {
        RuleFor(r => r).Custom((request, context) =>
        {
            try { request.Resolve(); }
            catch (ArgumentException exception) { context.AddFailure(exception.Message); }
        });
    }
}

using FarmStay.Application.Interfaces.Repositories.Reports;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using FluentValidation;

namespace FarmStay.Application.Services.Reports;
public abstract class ReportModuleService<T>(string key, IReportRepository<T> repository,
    IReportFileRenderer renderer, IValidator<ReportFilter> validator) : IReportModuleService<T> where T : class
{
    public async Task<ReportPage<T>> ListAsync(ReportScope scope, ReportFilter request, CancellationToken ct)
    {
        var query = await Prepare(scope, request, false, ct);
        var summary = await repository.GetSummaryAsync(query, ct);
        var rows = await repository.GetRowsAsync(query, request.Page, request.PageSize, ct);
        return new(ReportCatalog.Find(key).Columns, rows, summary.TotalRecords, request.Page,
            request.PageSize, request.Resolve().Label);
    }

    public async Task<ReportDownload> ExportAsync(ReportScope scope, ReportFilter request, string format, CancellationToken ct)
    {
        if (format is not ("pdf" or "excel")) throw new ReportException(ReportError.InvalidRequest, "Choose PDF or Excel.");
        var query = await Prepare(scope, request, true, ct);
        var limit = format == "pdf" ? 10000 : 100000;
        var summary = await repository.GetSummaryAsync(query, ct);
        if (summary.TotalRecords > limit) throw Limit(format, limit);
        // The extra row detects inserts between the summary and row queries without silent truncation.
        var rows = await repository.GetRowsAsync(query, 1, limit + 1, ct);
        if (rows.Count > limit) throw Limit(format, limit);
        return await renderer.RenderAsync(scope, ReportCatalog.Find(key), request, rows, format, ct);
    }

    private async Task<ReportQuery> Prepare(ReportScope scope, ReportFilter request, bool export, CancellationToken ct)
    {
        var permission = ReportCatalog.Find(key).Permission;
        if (!scope.GlobalAdministrator && (!scope.Permissions.Contains(permission) ||
            (export && !scope.Permissions.Contains(permission.Replace(".View", ".Export")))))
            throw new ReportException(ReportError.Forbidden, "You do not have access to this report.");
        await validator.ValidateAndThrowAsync(request, ct);
        var dates = request.Resolve();
        return new(scope.FarmHouseId, dates.Start, dates.End, string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            request.BookingType, request.Source);
    }
    private static ReportException Limit(string format, int limit) => new(ReportError.ExportLimit,
        $"Export limit: {limit:N0} rows for {format}. Narrow the date range or search and try again.");
}

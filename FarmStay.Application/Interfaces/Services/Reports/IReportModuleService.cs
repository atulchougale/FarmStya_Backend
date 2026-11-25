using FarmStay.Application.Reporting;

namespace FarmStay.Application.Interfaces.Services.Reports;
public interface IReportModuleService<T> where T : class
{
    Task<ReportPage<T>> ListAsync(ReportScope scope, ReportFilter request, CancellationToken ct);
    Task<ReportDownload> ExportAsync(ReportScope scope, ReportFilter request, string format, CancellationToken ct);
}

public interface IReportFileRenderer
{
    Task<ReportDownload> RenderAsync<T>(ReportScope scope, ReportDefinition definition,
        ReportFilter filter, IReadOnlyList<T> rows, string format, CancellationToken ct) where T : class;
}

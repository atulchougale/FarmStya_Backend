namespace FarmStay.Application.Reporting;
public interface IReportingService
{
    Task<IReadOnlyList<ReportCatalogEntry>> CatalogAsync(CancellationToken ct);
    Task<IReportPage> ListAsync(string report, ReportFilter filter, CancellationToken ct);
    Task<ReportDownload> ExportAsync(string report, ReportFilter filter, string format, CancellationToken ct);
}

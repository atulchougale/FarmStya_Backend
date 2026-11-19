namespace FarmStay.Application.Reporting;
public interface IReportingService
{
    Task<ReportPage> ListAsync(int farmhouse, string report, ReportFilter filter, CancellationToken ct);
    Task<ReportDownload> ExportAsync(int farmhouse, string report, ReportFilter filter, string format, CancellationToken ct);
}
public interface IReportAccess
{
    Task<IReadOnlySet<string>> PermissionsAsync(int farmhouse, int user, CancellationToken ct);
}

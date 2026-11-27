using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using Microsoft.Reporting.NETCore;

namespace FarmStay.Infrastructure.Reporting;
public sealed class ReportFileRenderer : IReportFileRenderer
{
    public Task<ReportDownload> RenderAsync<T>(ReportScope scope, ReportDefinition definition,
        ReportFilter filter, IReadOnlyList<T> rows, string format, CancellationToken ct) where T : class
    {
        ct.ThrowIfCancellationRequested();
        foreach (var column in definition.Columns)
            if (typeof(T).GetProperty(column.Key) == null)
                throw new InvalidOperationException($"Report field {column.Key} is missing from its typed response.");
        using var renderer = new LocalReport();
        using var template = typeof(ReportFileRenderer).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Reporting.Templates.{definition.Key}.rdlc")
            ?? throw new InvalidOperationException("The report template is missing.");
        renderer.LoadReportDefinition(template);
        renderer.DataSources.Add(new ReportDataSource("ReportData", rows));
        var selection = filter.Resolve().Label + (definition.Key == "bookings" ? $" | Type: {filter.BookingType} | Source: {filter.Source}" : "") +
            (string.IsNullOrWhiteSpace(filter.Search) ? "" : $" | Search: {filter.Search.Trim()}");
        renderer.SetParameters(new[] {
            new ReportParameter("Farmhouse", scope.FarmHouseName), new ReportParameter("Address", $"{scope.Address} | Contact: {scope.Contact}"),
            new ReportParameter("Selection", selection), new ReportParameter("Description", definition.Description),
            new ReportParameter("Generated", $"Generated: {DateTime.Now:dd MMM yyyy HH:mm} | Rows: {rows.Count:N0}") });
        var content = renderer.Render(format == "pdf" ? "PDF" : "EXCELOPENXML");
        return Task.FromResult(new ReportDownload(content,
            format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{definition.Key}-{DateTime.Now:yyyyMMdd-HHmmss}.{(format == "pdf" ? "pdf" : "xlsx")}"));
    }
}

using System.Data;
using System.Diagnostics;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Reporting.NETCore;

namespace FarmStay.Infrastructure.Reporting;
public sealed class ReportingService(AppDbContext db, ILogger<ReportingService> logger) : IReportingService
{
    private static readonly SemaphoreSlim RenderSlots = new(2);
    public async Task<ReportPage> ListAsync(int farmhouse, string report, ReportFilter filter, CancellationToken ct)
    {
        var definition = ReportCatalog.Find(report);
        var (table, total) = await Query(farmhouse, definition, filter, filter.Page, filter.PageSize, ct);
        var rows = table.Rows.Cast<DataRow>().Select(row => table.Columns.Cast<DataColumn>()
            .ToDictionary(column => column.ColumnName, column => row[column] == DBNull.Value ? null : (object?)row[column])).ToArray();
        return new(definition.Columns, rows, total, filter.Page, filter.PageSize, filter.Resolve().Label);
    }
    public async Task<ReportDownload> ExportAsync(int farmhouse, string report, ReportFilter filter, string format, CancellationToken ct)
    {
        var definition = ReportCatalog.Find(report);
        if (format is not ("pdf" or "excel")) throw new ArgumentException("Choose PDF or Excel.");
        await RenderSlots.WaitAsync(ct);
        try
        {
            // Bound the complete export, including data loading, under the rendering concurrency limit.
            var limit = format == "pdf" ? 10000 : 100000;
            var (table, total) = await Query(farmhouse, definition, filter, 1, limit + 1, ct);
            if (total > limit) throw new InvalidOperationException($"Export limit: {limit:N0} rows for {format}. Narrow the date range or search and try again.");
            var farm = await db.FarmHouses.AsNoTracking().SingleAsync(f => f.FarmHouseId == farmhouse, ct);
            ct.ThrowIfCancellationRequested();
            using var renderer = new LocalReport();
            using var template = typeof(ReportingService).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Reporting.Templates.{report}.rdlc")
                ?? throw new InvalidOperationException("The report template is missing.");
            renderer.LoadReportDefinition(template);
            renderer.DataSources.Add(new ReportDataSource("ReportData", table));
            var selection = filter.Resolve().Label + (report == "bookings" ? $" | Type: {filter.BookingType} | Source: {filter.Source}" : "") +
                (string.IsNullOrWhiteSpace(filter.Search) ? "" : $" | Search: {filter.Search.Trim()}");
            renderer.SetParameters(new[] {
                new ReportParameter("Farmhouse", farm.FarmHouseName), new ReportParameter("Address", $"{farm.Address} | Contact: {farm.MobileNumber}"),
                new ReportParameter("Selection", selection), new ReportParameter("Description", definition.Description),
                new ReportParameter("Generated", $"Generated: {DateTime.Now:dd MMM yyyy HH:mm} | Rows: {total:N0}") });
            var bytes = renderer.Render(format == "pdf" ? "PDF" : "EXCELOPENXML");
            return new(bytes, format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{report}-{DateTime.Now:yyyyMMdd-HHmmss}.{(format == "pdf" ? "pdf" : "xlsx")}");
        }
        finally { RenderSlots.Release(); }
    }
    private async Task<(DataTable Table, long Total)> Query(int farmhouse, ReportDefinition definition, ReportFilter filter,
        int page, int size, CancellationToken ct)
    {
        var period = filter.Resolve();
        var stopwatch = Stopwatch.StartNew();
        var connection = db.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = definition.Key switch {
                "bookings" => "dbo.usp_Report_Bookings", "revenue" => "dbo.usp_Report_Revenue",
                "customers" => "dbo.usp_Report_Customers", "occupancy" => "dbo.usp_Report_Occupancy", _ => throw new ArgumentException("Unknown report.") };
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 60;
            void Param(string name, DbType type, object? value, int? length = null) {
                var p = command.CreateParameter(); p.ParameterName = name; p.DbType = type; p.Value = value ?? DBNull.Value;
                if (length.HasValue) p.Size = length.Value; command.Parameters.Add(p);
            }
            Param("@FarmHouseId", DbType.Int32, farmhouse);
            Param("@FromDate", DbType.DateTime2, period.Start); Param("@ToDateExclusive", DbType.DateTime2, period.End);
            Param("@Search", DbType.String, string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim(), 100);
            Param("@BookingType", DbType.String, filter.BookingType, 10); Param("@Source", DbType.String, filter.Source, 10);
            Param("@Page", DbType.Int32, page); Param("@PageSize", DbType.Int32, size);
            await using var reader = await command.ExecuteReaderAsync(ct);
            var table = new DataTable("ReportData");
            for (var i = 0; i < reader.FieldCount; i++) table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            while (await reader.ReadAsync(ct)) { var values = new object[reader.FieldCount]; reader.GetValues(values); table.Rows.Add(values); }
            if (!await reader.NextResultAsync(ct) || !await reader.ReadAsync(ct)) throw new InvalidOperationException("Report count is missing.");
            var total = reader.GetInt64(0);
            logger.LogInformation("Report {Report} SQL completed for farmhouse {Farmhouse}: {Rows} page rows, {Total} matches, {Elapsed} ms.",
                definition.Key, farmhouse, table.Rows.Count, total, stopwatch.ElapsedMilliseconds);
            return (table, total);
        }
        finally { if (close) await connection.CloseAsync(); }
    }
}

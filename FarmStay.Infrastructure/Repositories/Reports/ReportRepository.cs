using System.Diagnostics;
using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.Repositories.Reports;
public abstract class ReportRepository(AppDbContext db, ILogger logger)
{
    protected async Task<IReadOnlyList<T>> ExecuteAsync<T>(FormattableString sql, string procedure, CancellationToken ct) where T : class
    {
        var elapsed = Stopwatch.StartNew();
        var previousTimeout = db.Database.GetCommandTimeout();
        try
        {
            db.Database.SetCommandTimeout(60);
            // Materialize EXEC first: do not compose Single/Where/Count over a non-composable SQL Server procedure.
            var rows = await db.Set<T>().FromSqlInterpolated(sql).AsNoTracking().ToListAsync(ct);
            logger.LogInformation("Report SP {Procedure} returned {RowCount} rows in {ElapsedMs} ms.", procedure, rows.Count, elapsed.ElapsedMilliseconds);
            return rows;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Report SP {Procedure} failed after {ElapsedMs} ms.", procedure, elapsed.ElapsedMilliseconds);
            throw new InvalidOperationException("Report data could not be loaded. Please retry or contact administration.", exception);
        }
        finally { db.Database.SetCommandTimeout(previousTimeout); }
    }
}

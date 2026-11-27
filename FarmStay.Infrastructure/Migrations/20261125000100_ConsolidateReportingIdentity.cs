using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261125000100_ConsolidateReportingIdentity")]
public sealed class ConsolidateReportingIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Read("Up"));
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Read("Down"));
    private static string Read(string direction)
    {
        using var stream = typeof(ConsolidateReportingIdentity).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.SQL.Reports.Consolidation.{direction}.sql")
            ?? throw new InvalidOperationException("Reporting consolidation SQL resource is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
public partial class StandardStoredProcedureIntegration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var module in new[] { "Booking", "Revenue", "Customer", "Occupancy" })
        foreach (var action in new[] { "GetReport", "GetSummary" })
        {
            using var stream = typeof(StandardStoredProcedureIntegration).Assembly.GetManifestResourceStream(
                $"FarmStay.Infrastructure.Reporting.Sql.Standard.{module}_{action}.sql")
                ?? throw new InvalidOperationException("Standard reporting procedure resource is missing.");
            using var reader = new StreamReader(stream);
            migrationBuilder.Sql(reader.ReadToEnd());
        }
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var module in new[] { "Booking", "Revenue", "Customer", "Occupancy" })
        foreach (var action in new[] { "GetReport", "GetSummary" })
            migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS dbo.usp_{module}_{action};");
    }
}

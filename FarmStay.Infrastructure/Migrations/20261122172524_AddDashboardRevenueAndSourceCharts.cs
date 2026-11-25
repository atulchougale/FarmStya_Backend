using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
public partial class AddDashboardRevenueAndSourceCharts : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  foreach (var name in new[] { "RevenueTrend", "BookingSources" })
  {
   using var stream = typeof(AddDashboardRevenueAndSourceCharts).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Reporting.Sql.Dashboard.{name}.sql")
    ?? throw new InvalidOperationException("Dashboard chart SQL resource is missing.");
   using var reader = new StreamReader(stream);
   migrationBuilder.Sql(reader.ReadToEnd());
  }
 }
 protected override void Down(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Dashboard_GetRevenueTrend; DROP PROCEDURE IF EXISTS dbo.usp_Dashboard_GetBookingSources;");
 }
}

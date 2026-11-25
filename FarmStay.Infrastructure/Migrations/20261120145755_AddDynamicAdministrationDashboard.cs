using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
public partial class AddDynamicAdministrationDashboard : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  foreach(var name in new[]{"Permissions","Summary","Calendar"})
  {
   using var stream=typeof(AddDynamicAdministrationDashboard).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Reporting.Sql.Dashboard.{name}.sql")
    ?? throw new InvalidOperationException("Dashboard SQL resource is missing.");
   using var reader=new StreamReader(stream);migrationBuilder.Sql(reader.ReadToEnd());
  }
 }
 protected override void Down(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Dashboard_GetSummary; DROP PROCEDURE IF EXISTS dbo.usp_Dashboard_GetCalendar;");
 }
}

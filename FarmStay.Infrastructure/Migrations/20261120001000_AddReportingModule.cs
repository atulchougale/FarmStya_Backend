using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261120001000_AddReportingModule")]
public sealed class AddReportingModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "SeedAndIndexes", "Bookings", "Revenue", "Customers", "Occupancy" })
        {
            using var stream = typeof(AddReportingModule).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Reporting.Sql.{name}.sql")
                ?? throw new InvalidOperationException("Reporting SQL resource is missing.");
            using var reader = new StreamReader(stream);
            migrationBuilder.Sql(reader.ReadToEnd());
        }
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var report in new[] { "Bookings", "Revenue", "Customers", "Occupancy" })
            migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS dbo.usp_Report_{report};");
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS IX_Reporting_Bookings_Created ON dbo.Bookings;
            DROP INDEX IF EXISTS IX_Reporting_Payments_Received ON dbo.PaymentTransactions;
            DROP INDEX IF EXISTS IX_Reporting_StayDates ON dbo.BookingStayDates;
            DELETE x FROM dbo.RoleModulePermissions x JOIN dbo.Permissions p ON p.PermissionId=x.PermissionId
            WHERE p.PermissionName IN ('BookingReport.View','BookingReport.Export','RevenueReport.View','RevenueReport.Export','CustomerReport.View','CustomerReport.Export','OccupancyReport.View','OccupancyReport.Export');
            DELETE dbo.Permissions WHERE PermissionName IN ('BookingReport.View','BookingReport.Export','RevenueReport.View','RevenueReport.Export','CustomerReport.View','CustomerReport.Export','OccupancyReport.View','OccupancyReport.Export');
            """);
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "ModuleId", "CreatedBy", "CreatedDate", "DisplayName", "DisplayOrder", "Icon", "IsActive", "ModifiedBy", "ModifiedDate", "ModuleName", "Route" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8084), "Dashboard", 1, null, true, null, null, "Dashboard", "/dashboard" },
                    { 2, null, new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8089), "Bookings", 2, null, true, null, null, "Bookings", "/bookings" },
                    { 3, null, new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8092), "Accounts / Finance", 3, null, true, null, null, "Accounts", "/accounts" },
                    { 4, null, new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8094), "Reporting", 4, null, true, null, null, "Reporting", "/reporting" },
                    { 5, null, new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8096), "Website Settings", 5, null, true, null, null, "WebsiteSettings", "/website-settings" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5);
        }
    }
}

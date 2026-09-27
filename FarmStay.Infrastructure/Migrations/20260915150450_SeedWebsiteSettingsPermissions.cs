using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedWebsiteSettingsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(5487));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(5489));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(5491));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(5494));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "CreatedBy", "CreatedDate", "Description", "DisplayOrder", "IsActive", "IsSystemPermission", "ModifiedBy", "ModifiedDate", "PermissionName" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6301), "View gallery items", 1, true, true, null, null, "Gallery.View" },
                    { 2, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6307), "Create gallery items", 2, true, true, null, null, "Gallery.Create" },
                    { 3, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6310), "Edit gallery items", 3, true, true, null, null, "Gallery.Edit" },
                    { 4, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6311), "Delete gallery items", 4, true, true, null, null, "Gallery.Delete" },
                    { 5, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6313), "View amenities", 5, true, true, null, null, "Amenity.View" },
                    { 6, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6314), "Create amenities", 6, true, true, null, null, "Amenity.Create" },
                    { 7, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6316), "Edit amenities", 7, true, true, null, null, "Amenity.Edit" },
                    { 8, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6317), "Delete amenities", 8, true, true, null, null, "Amenity.Delete" },
                    { 9, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6319), "View customer feedback and reviews", 9, true, true, null, null, "Feedback.View" },
                    { 10, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6320), "Create customer feedback", 10, true, true, null, null, "Feedback.Create" },
                    { 11, null, new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6322), "Delete customer feedback", 11, true, true, null, null, "Feedback.Delete" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8084));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8089));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8092));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8094));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 14, 12, 34, 350, DateTimeKind.Utc).AddTicks(8096));
        }
    }
}

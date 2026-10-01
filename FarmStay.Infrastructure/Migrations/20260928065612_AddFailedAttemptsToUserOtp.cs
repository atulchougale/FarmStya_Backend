using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedAttemptsToUserOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "UserOtps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContactDetail",
                columns: table => new
                {
                    ContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetail", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_ContactDetail_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5239));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5256));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5262));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5267));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5272));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6686));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6697));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6703));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6708));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6711));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6715));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6719));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6727));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6731));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6735));

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetail_FarmHouseId",
                table: "ContactDetail",
                column: "FarmHouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactDetail");

            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "UserOtps");

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

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6301));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6307));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6310));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6311));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6313));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6314));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6316));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6317));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6319));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6320));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 15, 15, 4, 49, 975, DateTimeKind.Utc).AddTicks(6322));
        }
    }
}

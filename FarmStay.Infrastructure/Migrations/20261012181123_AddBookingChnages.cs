using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingChnages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingLog_Booking_BookingId",
                table: "BookingLog");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingLog_Users_UserId",
                table: "BookingLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingLog",
                table: "BookingLog");

            migrationBuilder.RenameTable(
                name: "BookingLog",
                newName: "BookingLogs");

            migrationBuilder.RenameIndex(
                name: "IX_BookingLog_UserId",
                table: "BookingLogs",
                newName: "IX_BookingLogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingLog_BookingId",
                table: "BookingLogs",
                newName: "IX_BookingLogs_BookingId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDecorationRequired",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEvent",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingLogs",
                table: "BookingLogs",
                column: "BookingLogId");

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(1656));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(1662));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(1664));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(1666));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(1668));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2125));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2129));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2132));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2133));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2136));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2138));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2139));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2141));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2142));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2144));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 12, 18, 11, 22, 216, DateTimeKind.Utc).AddTicks(2146));

            migrationBuilder.AddForeignKey(
                name: "FK_BookingLogs_Booking_BookingId",
                table: "BookingLogs",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingLogs_Users_UserId",
                table: "BookingLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingLogs_Booking_BookingId",
                table: "BookingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingLogs_Users_UserId",
                table: "BookingLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingLogs",
                table: "BookingLogs");

            migrationBuilder.DropColumn(
                name: "IsDecorationRequired",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "IsEvent",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "BookingLogs",
                newName: "BookingLog");

            migrationBuilder.RenameIndex(
                name: "IX_BookingLogs_UserId",
                table: "BookingLog",
                newName: "IX_BookingLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingLogs_BookingId",
                table: "BookingLog",
                newName: "IX_BookingLog_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingLog",
                table: "BookingLog",
                column: "BookingLogId");

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2211));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2219));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2222));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2223));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2225));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2697));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2703));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2706));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2707));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2709));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2710));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2711));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2713));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2714));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2716));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 11, 16, 21, 10, 546, DateTimeKind.Utc).AddTicks(2717));

            migrationBuilder.AddForeignKey(
                name: "FK_BookingLog_Booking_BookingId",
                table: "BookingLog",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingLog_Users_UserId",
                table: "BookingLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

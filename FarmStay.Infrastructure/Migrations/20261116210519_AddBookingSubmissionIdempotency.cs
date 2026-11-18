using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSubmissionIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Bookings",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingVersion",
                table: "Bookings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmitIdempotencyKey",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitRequestHash",
                table: "Bookings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FarmHouseId_CustomerUserId_SubmitIdempotencyKey",
                table: "Bookings",
                columns: new[] { "FarmHouseId", "CustomerUserId", "SubmitIdempotencyKey" },
                unique: true,
                filter: "[SubmitIdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_FarmHouseId_CustomerUserId_SubmitIdempotencyKey",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PricingVersion",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SubmitIdempotencyKey",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SubmitRequestHash",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6530));
        }
    }
}

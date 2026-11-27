using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecureFileOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS(SELECT 1 FROM dbo.BookingDocuments d LEFT JOIN dbo.Bookings b ON b.BookingId=d.BookingId AND b.FarmHouseId=d.FarmHouseId AND b.CustomerUserId=d.UserId WHERE b.BookingId IS NULL)
                  THROW 51033,'Document ownership must be repaired before adding file constraints.',1;
                IF EXISTS(SELECT 1 FROM dbo.AccountingAttachments a LEFT JOIN dbo.AccountingVouchers v ON v.Id=a.VoucherId AND v.FarmHouseId=a.FarmHouseId WHERE v.Id IS NULL)
                  THROW 51033,'Attachment ownership must be repaired before adding file constraints.',1;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingAttachments_AccountingVouchers_VoucherId",
                table: "AccountingAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingDocuments_Bookings_BookingId",
                table: "BookingDocuments");

migrationBuilder.DropIndex(
                name: "IX_AccountingAttachments_VoucherId",
                table: "AccountingAttachments");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Bookings_FarmHouseId_BookingId_CustomerUserId",
                table: "Bookings",
                columns: new[] { "FarmHouseId", "BookingId", "CustomerUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDocuments_FarmHouseId_BookingId_UserId",
                table: "BookingDocuments",
                columns: new[] { "FarmHouseId", "BookingId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAttachments_FarmHouseId_VoucherId",
                table: "AccountingAttachments",
                columns: new[] { "FarmHouseId", "VoucherId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingAttachments_AccountingVouchers_FarmHouseId_VoucherId",
                table: "AccountingAttachments",
                columns: new[] { "FarmHouseId", "VoucherId" },
                principalTable: "AccountingVouchers",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingDocuments_Bookings_FarmHouseId_BookingId_UserId",
                table: "BookingDocuments",
                columns: new[] { "FarmHouseId", "BookingId", "UserId" },
                principalTable: "Bookings",
                principalColumns: new[] { "FarmHouseId", "BookingId", "CustomerUserId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingAttachments_AccountingVouchers_FarmHouseId_VoucherId",
                table: "AccountingAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingDocuments_Bookings_FarmHouseId_BookingId_UserId",
                table: "BookingDocuments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Bookings_FarmHouseId_BookingId_CustomerUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingDocuments_FarmHouseId_BookingId_UserId",
                table: "BookingDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AccountingAttachments_FarmHouseId_VoucherId",
                table: "AccountingAttachments");

migrationBuilder.CreateIndex(
                name: "IX_AccountingAttachments_VoucherId",
                table: "AccountingAttachments",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingAttachments_AccountingVouchers_VoucherId",
                table: "AccountingAttachments",
                column: "VoucherId",
                principalTable: "AccountingVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingDocuments_Bookings_BookingId",
                table: "BookingDocuments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

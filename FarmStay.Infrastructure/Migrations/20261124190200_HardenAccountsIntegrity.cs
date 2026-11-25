using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenAccountsIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_PaymentModeId",
                table: "AccountingVouchers",
                column: "PaymentModeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AccountingVoucher_Balanced",
                table: "AccountingVouchers",
                sql: "[TotalDebit] = [TotalCredit] AND [TotalDebit] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingVouchers_AccountingMasters_PaymentModeId",
                table: "AccountingVouchers",
                column: "PaymentModeId",
                principalTable: "AccountingMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingVouchers_AccountingMasters_PaymentModeId",
                table: "AccountingVouchers");

            migrationBuilder.DropIndex(
                name: "IX_AccountingVouchers_PaymentModeId",
                table: "AccountingVouchers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AccountingVoucher_Balanced",
                table: "AccountingVouchers");
        }
    }
}

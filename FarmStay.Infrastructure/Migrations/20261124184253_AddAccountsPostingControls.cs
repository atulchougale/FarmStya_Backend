using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsPostingControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PaymentModeId",
                table: "AccountingVouchers",
                type: "bigint",
                nullable: true);
            foreach(var script in new[]{"ImmutableVoucher","ImmutableLine","ImmutableAudit"}) {
                using var stream=typeof(AddAccountsPostingControls).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.Accounts.Sql.{script}.sql");
                using var reader=new System.IO.StreamReader(stream!); migrationBuilder.Sql(reader.ReadToEnd());
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.tr_AccountingVoucher_Immutable; DROP TRIGGER IF EXISTS dbo.tr_AccountingLine_Immutable; DROP TRIGGER IF EXISTS dbo.tr_AccountingAudit_Immutable;");
            migrationBuilder.DropColumn(
                name: "PaymentModeId",
                table: "AccountingVouchers");
        }
    }
}

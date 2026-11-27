using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StabilizeAccountsFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Sql("Preflight"));
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingAssetEvents_AccountingFixedAssets_AssetId",
                table: "AccountingAssetEvents");

            migrationBuilder.DropIndex(
                name: "IX_AccountingAssetEvents_AssetId",
                table: "AccountingAssetEvents");

            migrationBuilder.AddColumn<long>(
                name: "MoneyAccountId",
                table: "PaymentTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OriginalPaymentTransactionId",
                table: "PaymentTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundAccountingKind",
                table: "PaymentTransactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "PaymentTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReference",
                table: "PaymentTransactions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundRequestHash",
                table: "PaymentTransactions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DefaultBankMoneyAccountId",
                table: "AccountingSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DefaultCashMoneyAccountId",
                table: "AccountingSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitializationVersion",
                table: "AccountingSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PaymentTransactions_FarmHouseId_PaymentTransactionId",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "PaymentTransactionId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_LedgerAccounts_FarmHouseId_Id",
                table: "LedgerAccounts",
                columns: new[] { "FarmHouseId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AccountingVouchers_FarmHouseId_Id",
                table: "AccountingVouchers",
                columns: new[] { "FarmHouseId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AccountingVendors_FarmHouseId_Id",
                table: "AccountingVendors",
                columns: new[] { "FarmHouseId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AccountingMoneyAccounts_FarmHouseId_Id",
                table: "AccountingMoneyAccounts",
                columns: new[] { "FarmHouseId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AccountingFixedAssets_FarmHouseId_Id",
                table: "AccountingFixedAssets",
                columns: new[] { "FarmHouseId", "Id" });

            migrationBuilder.CreateTable(
                name: "AccountingSourceReviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceKind = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingSourceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingSourceReviews_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_FarmHouseId_MoneyAccountId",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "MoneyAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_FarmHouseId_OriginalPaymentTransactionId",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "OriginalPaymentTransactionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_FarmHouseId_DefaultBankMoneyAccountId",
                table: "AccountingSettings",
                columns: new[] { "FarmHouseId", "DefaultBankMoneyAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_FarmHouseId_DefaultCashMoneyAccountId",
                table: "AccountingSettings",
                columns: new[] { "FarmHouseId", "DefaultCashMoneyAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_CreditAccountId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "CreditAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_ExpenseAccountId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "ExpenseAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_VendorId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "VendorId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId_AssetId",
                table: "AccountingAssetEvents",
                columns: new[] { "FarmHouseId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId_VoucherId",
                table: "AccountingAssetEvents",
                columns: new[] { "FarmHouseId", "VoucherId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSourceReviews_FarmHouseId",
                table: "AccountingSourceReviews",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSourceReviews_FarmHouseId_SourceKey",
                table: "AccountingSourceReviews",
                columns: new[] { "FarmHouseId", "SourceKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingAssetEvents_AccountingFixedAssets_FarmHouseId_AssetId",
                table: "AccountingAssetEvents",
                columns: new[] { "FarmHouseId", "AssetId" },
                principalTable: "AccountingFixedAssets",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingAssetEvents_AccountingVouchers_FarmHouseId_VoucherId",
                table: "AccountingAssetEvents",
                columns: new[] { "FarmHouseId", "VoucherId" },
                principalTable: "AccountingVouchers",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingBankStatements_AccountingVoucherLines_VoucherLineId",
                table: "AccountingBankStatements",
                column: "VoucherLineId",
                principalTable: "AccountingVoucherLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingBankStatements_LedgerAccounts_FarmHouseId_AccountId",
                table: "AccountingBankStatements",
                columns: new[] { "FarmHouseId", "AccountId" },
                principalTable: "LedgerAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingRecurringExpenses_AccountingVendors_FarmHouseId_VendorId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "VendorId" },
                principalTable: "AccountingVendors",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingRecurringExpenses_LedgerAccounts_FarmHouseId_CreditAccountId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "CreditAccountId" },
                principalTable: "LedgerAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingRecurringExpenses_LedgerAccounts_FarmHouseId_ExpenseAccountId",
                table: "AccountingRecurringExpenses",
                columns: new[] { "FarmHouseId", "ExpenseAccountId" },
                principalTable: "LedgerAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingSettings_AccountingMoneyAccounts_FarmHouseId_DefaultBankMoneyAccountId",
                table: "AccountingSettings",
                columns: new[] { "FarmHouseId", "DefaultBankMoneyAccountId" },
                principalTable: "AccountingMoneyAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingSettings_AccountingMoneyAccounts_FarmHouseId_DefaultCashMoneyAccountId",
                table: "AccountingSettings",
                columns: new[] { "FarmHouseId", "DefaultCashMoneyAccountId" },
                principalTable: "AccountingMoneyAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_AccountingMoneyAccounts_FarmHouseId_MoneyAccountId",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "MoneyAccountId" },
                principalTable: "AccountingMoneyAccounts",
                principalColumns: new[] { "FarmHouseId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_PaymentTransactions_FarmHouseId_OriginalPaymentTransactionId",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "OriginalPaymentTransactionId" },
                principalTable: "PaymentTransactions",
                principalColumns: new[] { "FarmHouseId", "PaymentTransactionId" },
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.CreateIndex(name:"IX_Payment_ActualRefundReference",table:"PaymentTransactions",columns:new[]{"FarmHouseId","MoneyAccountId","RefundReference"},unique:true,filter:"[PaymentType]='Refund' AND [RefundReference] IS NOT NULL AND [IsDelete]=0");
            migrationBuilder.Sql(Sql("BankGuard"));
            migrationBuilder.Sql(Sql("Receipt"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS(SELECT 1 FROM dbo.PaymentTransactions WHERE OriginalPaymentTransactionId IS NOT NULL OR MoneyAccountId IS NOT NULL) OR EXISTS(SELECT 1 FROM dbo.AccountingSourceReviews) THROW 51034,'Finance audit data exists; reconcile and archive it before a reviewed rollback.',1;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_AccountingBankStatementTenant;");
            migrationBuilder.DropIndex(name:"IX_Payment_ActualRefundReference",table:"PaymentTransactions");
            migrationBuilder.Sql(Sql("ReceiptBefore"));
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingAssetEvents_AccountingFixedAssets_FarmHouseId_AssetId",
                table: "AccountingAssetEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingAssetEvents_AccountingVouchers_FarmHouseId_VoucherId",
                table: "AccountingAssetEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingBankStatements_AccountingVoucherLines_VoucherLineId",
                table: "AccountingBankStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingBankStatements_LedgerAccounts_FarmHouseId_AccountId",
                table: "AccountingBankStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingRecurringExpenses_AccountingVendors_FarmHouseId_VendorId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingRecurringExpenses_LedgerAccounts_FarmHouseId_CreditAccountId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingRecurringExpenses_LedgerAccounts_FarmHouseId_ExpenseAccountId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingSettings_AccountingMoneyAccounts_FarmHouseId_DefaultBankMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingSettings_AccountingMoneyAccounts_FarmHouseId_DefaultCashMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_AccountingMoneyAccounts_FarmHouseId_MoneyAccountId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_PaymentTransactions_FarmHouseId_OriginalPaymentTransactionId",
                table: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "AccountingSourceReviews");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PaymentTransactions_FarmHouseId_PaymentTransactionId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_FarmHouseId_MoneyAccountId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_FarmHouseId_OriginalPaymentTransactionId",
                table: "PaymentTransactions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_LedgerAccounts_FarmHouseId_Id",
                table: "LedgerAccounts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AccountingVouchers_FarmHouseId_Id",
                table: "AccountingVouchers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AccountingVendors_FarmHouseId_Id",
                table: "AccountingVendors");

            migrationBuilder.DropIndex(
                name: "IX_AccountingSettings_FarmHouseId_DefaultBankMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropIndex(
                name: "IX_AccountingSettings_FarmHouseId_DefaultCashMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_CreditAccountId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_ExpenseAccountId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId_VendorId",
                table: "AccountingRecurringExpenses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AccountingMoneyAccounts_FarmHouseId_Id",
                table: "AccountingMoneyAccounts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AccountingFixedAssets_FarmHouseId_Id",
                table: "AccountingFixedAssets");

            migrationBuilder.DropIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId_AssetId",
                table: "AccountingAssetEvents");

            migrationBuilder.DropIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId_VoucherId",
                table: "AccountingAssetEvents");

            migrationBuilder.DropColumn(
                name: "MoneyAccountId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "OriginalPaymentTransactionId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundAccountingKind",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundRequestHash",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "DefaultBankMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropColumn(
                name: "DefaultCashMoneyAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropColumn(
                name: "InitializationVersion",
                table: "AccountingSettings");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_AssetId",
                table: "AccountingAssetEvents",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingAssetEvents_AccountingFixedAssets_AssetId",
                table: "AccountingAssetEvents",
                column: "AssetId",
                principalTable: "AccountingFixedAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
        private static string Sql(string name)
        {
            using var stream=typeof(StabilizeAccountsFinance).Assembly.GetManifestResourceStream($"FarmStay.Infrastructure.SQL.Accounts.Phase4.{name}.sql")??throw new InvalidOperationException("Finance migration SQL resource is missing.");
            using var reader=new System.IO.StreamReader(stream);return reader.ReadToEnd();
        }
    }
}

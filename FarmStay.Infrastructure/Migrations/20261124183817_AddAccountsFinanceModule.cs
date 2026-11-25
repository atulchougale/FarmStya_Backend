using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsFinanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingAudits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingAudits_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingBankStatements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    StatementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VoucherLineId = table.Column<long>(type: "bigint", nullable: true),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingBankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingBankStatements_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingFinancialYears",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingFinancialYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingFinancialYears_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GroupCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultLedgerCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingMasters_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingRecurringExpenses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpenseAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreditAccountId = table.Column<long>(type: "bigint", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IntervalMonths = table.Column<int>(type: "int", nullable: false),
                    NextDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingRecurringExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingRecurringExpenses_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GstEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GstNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CapitalisationThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RevenuePolicy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    NormalBalance = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxAccount = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerAccounts_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerAccounts_LedgerAccounts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "LedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialYearId = table.Column<long>(type: "bigint", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriods_AccountingFinancialYears_FinancialYearId",
                        column: x => x.FinancialYearId,
                        principalTable: "AccountingFinancialYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingPeriods_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingMoneyAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumberLast4 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ifsc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingMoneyAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingMoneyAccounts_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingMoneyAccounts_LedgerAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "LedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingVendors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GstNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingVendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingVendors_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVendors_LedgerAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "LedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingVouchers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VoucherType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinancialYearId = table.Column<long>(type: "bigint", nullable: false),
                    AccountingPeriodId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversalOfVoucherId = table.Column<long>(type: "bigint", nullable: true),
                    SettlesVoucherId = table.Column<long>(type: "bigint", nullable: true),
                    PostedBy = table.Column<int>(type: "int", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingVouchers_AccountingFinancialYears_FinancialYearId",
                        column: x => x.FinancialYearId,
                        principalTable: "AccountingFinancialYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVouchers_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVouchers_AccountingVouchers_ReversalOfVoucherId",
                        column: x => x.ReversalOfVoucherId,
                        principalTable: "AccountingVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVouchers_AccountingVouchers_SettlesVoucherId",
                        column: x => x.SettlesVoucherId,
                        principalTable: "AccountingVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVouchers_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingAttachments_AccountingVouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "AccountingVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingAttachments_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingFixedAssets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    AssetTypeId = table.Column<long>(type: "bigint", nullable: true),
                    VendorId = table.Column<long>(type: "bigint", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ResidualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false),
                    DepreciationMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnnualRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WarrantyEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AcquisitionVoucherId = table.Column<long>(type: "bigint", nullable: false),
                    DisposalVoucherId = table.Column<long>(type: "bigint", nullable: true),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingFixedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingFixedAssets_AccountingMasters_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AccountingMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingFixedAssets_AccountingMasters_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AccountingMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingFixedAssets_AccountingVouchers_AcquisitionVoucherId",
                        column: x => x.AcquisitionVoucherId,
                        principalTable: "AccountingVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingFixedAssets_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingVoucherLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherId = table.Column<long>(type: "bigint", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VendorId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CostCentreId = table.Column<long>(type: "bigint", nullable: true),
                    TaxId = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    ReconciledDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingVoucherLines", x => x.Id);
                    table.CheckConstraint("CK_AccountingVoucherLine_OneSide", "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_AccountingMasters_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AccountingMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_AccountingMasters_CostCentreId",
                        column: x => x.CostCentreId,
                        principalTable: "AccountingMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_AccountingMasters_TaxId",
                        column: x => x.TaxId,
                        principalTable: "AccountingMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_AccountingVendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "AccountingVendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_AccountingVouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "AccountingVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingVoucherLines_LedgerAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "LedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountingAssetEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VoucherId = table.Column<long>(type: "bigint", nullable: true),
                    EventKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingAssetEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingAssetEvents_AccountingFixedAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "AccountingFixedAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingAssetEvents_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_AssetId",
                table: "AccountingAssetEvents",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId",
                table: "AccountingAssetEvents",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAssetEvents_FarmHouseId_EventKey",
                table: "AccountingAssetEvents",
                columns: new[] { "FarmHouseId", "EventKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAttachments_FarmHouseId",
                table: "AccountingAttachments",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAttachments_VoucherId",
                table: "AccountingAttachments",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingAudits_FarmHouseId",
                table: "AccountingAudits",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingBankStatements_FarmHouseId",
                table: "AccountingBankStatements",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingBankStatements_FarmHouseId_AccountId_Reference_StatementDate",
                table: "AccountingBankStatements",
                columns: new[] { "FarmHouseId", "AccountId", "Reference", "StatementDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingBankStatements_VoucherLineId",
                table: "AccountingBankStatements",
                column: "VoucherLineId",
                unique: true,
                filter: "[VoucherLineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFinancialYears_FarmHouseId",
                table: "AccountingFinancialYears",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFinancialYears_FarmHouseId_FromDate",
                table: "AccountingFinancialYears",
                columns: new[] { "FarmHouseId", "FromDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_AcquisitionVoucherId",
                table: "AccountingFixedAssets",
                column: "AcquisitionVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_AssetTypeId",
                table: "AccountingFixedAssets",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_CategoryId",
                table: "AccountingFixedAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_FarmHouseId",
                table: "AccountingFixedAssets",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_FarmHouseId_Code",
                table: "AccountingFixedAssets",
                columns: new[] { "FarmHouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMasters_FarmHouseId_Kind_Code",
                table: "AccountingMasters",
                columns: new[] { "FarmHouseId", "Kind", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMoneyAccounts_AccountId",
                table: "AccountingMoneyAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMoneyAccounts_FarmHouseId",
                table: "AccountingMoneyAccounts",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMoneyAccounts_FarmHouseId_Code",
                table: "AccountingMoneyAccounts",
                columns: new[] { "FarmHouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_FarmHouseId",
                table: "AccountingPeriods",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_FarmHouseId_FromDate",
                table: "AccountingPeriods",
                columns: new[] { "FarmHouseId", "FromDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_FinancialYearId",
                table: "AccountingPeriods",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecurringExpenses_FarmHouseId",
                table: "AccountingRecurringExpenses",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_FarmHouseId",
                table: "AccountingSettings",
                column: "FarmHouseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVendors_AccountId",
                table: "AccountingVendors",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVendors_FarmHouseId",
                table: "AccountingVendors",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVendors_FarmHouseId_Code",
                table: "AccountingVendors",
                columns: new[] { "FarmHouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_AccountId_VoucherId",
                table: "AccountingVoucherLines",
                columns: new[] { "AccountId", "VoucherId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_CategoryId",
                table: "AccountingVoucherLines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_CostCentreId",
                table: "AccountingVoucherLines",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_TaxId",
                table: "AccountingVoucherLines",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_VendorId",
                table: "AccountingVoucherLines",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVoucherLines_VoucherId_LineNumber",
                table: "AccountingVoucherLines",
                columns: new[] { "VoucherId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_AccountingPeriodId",
                table: "AccountingVouchers",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_FarmHouseId",
                table: "AccountingVouchers",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_FarmHouseId_SourceKey",
                table: "AccountingVouchers",
                columns: new[] { "FarmHouseId", "SourceKey" },
                unique: true,
                filter: "[SourceKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_FarmHouseId_VoucherDate_Status",
                table: "AccountingVouchers",
                columns: new[] { "FarmHouseId", "VoucherDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_FarmHouseId_VoucherNumber",
                table: "AccountingVouchers",
                columns: new[] { "FarmHouseId", "VoucherNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_FinancialYearId",
                table: "AccountingVouchers",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_ReversalOfVoucherId",
                table: "AccountingVouchers",
                column: "ReversalOfVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingVouchers_SettlesVoucherId",
                table: "AccountingVouchers",
                column: "SettlesVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_FarmHouseId",
                table: "LedgerAccounts",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_FarmHouseId_Code",
                table: "LedgerAccounts",
                columns: new[] { "FarmHouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_ParentId",
                table: "LedgerAccounts",
                column: "ParentId");
            using(var stream=typeof(AddAccountsFinanceModule).Assembly.GetManifestResourceStream("FarmStay.Infrastructure.Accounts.Sql.AccountsSeeds.sql"))
            using(var reader=new System.IO.StreamReader(stream!)) migrationBuilder.Sql(reader.ReadToEnd());
            using(var stream=typeof(AddAccountsFinanceModule).Assembly.GetManifestResourceStream("FarmStay.Infrastructure.Accounts.Sql.AccountsReport.sql"))
            using(var reader=new System.IO.StreamReader(stream!)) migrationBuilder.Sql(reader.ReadToEnd());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Accounts_Report");
            migrationBuilder.DropTable(
                name: "AccountingAssetEvents");

            migrationBuilder.DropTable(
                name: "AccountingAttachments");

            migrationBuilder.DropTable(
                name: "AccountingAudits");

            migrationBuilder.DropTable(
                name: "AccountingBankStatements");

            migrationBuilder.DropTable(
                name: "AccountingMoneyAccounts");

            migrationBuilder.DropTable(
                name: "AccountingRecurringExpenses");

            migrationBuilder.DropTable(
                name: "AccountingSettings");

            migrationBuilder.DropTable(
                name: "AccountingVoucherLines");

            migrationBuilder.DropTable(
                name: "AccountingFixedAssets");

            migrationBuilder.DropTable(
                name: "AccountingVendors");

            migrationBuilder.DropTable(
                name: "AccountingMasters");

            migrationBuilder.DropTable(
                name: "AccountingVouchers");

            migrationBuilder.DropTable(
                name: "LedgerAccounts");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "AccountingFinancialYears");
        }
    }
}

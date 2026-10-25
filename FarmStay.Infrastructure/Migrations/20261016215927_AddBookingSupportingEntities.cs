using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSupportingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingDateReservations",
                columns: table => new
                {
                    ReservationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDateReservations", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_BookingDateReservations_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingDateReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    CustomerUserId = table.Column<int>(type: "int", nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdultCount = table.Column<int>(type: "int", nullable: false),
                    ChildrenCount = table.Column<int>(type: "int", nullable: false),
                    PersonCount = table.Column<int>(type: "int", nullable: false),
                    CustomerNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "date", nullable: false),
                    StayDays = table.Column<int>(type: "int", nullable: false),
                    BookingType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EventAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceDailyRateSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingDailyRateSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EventDailyRateSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OwnerDeadlineUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CustomerDeadlineUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ExtensionUsed = table.Column<bool>(type: "bit", nullable: false),
                    ExtensionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarmHouseCancellationPolicies",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    PolicyVersion = table.Column<int>(type: "int", nullable: false),
                    PendingCancelAllowed = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedCancelAllowed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedAdvanceRefundable = table.Column<bool>(type: "bit", nullable: false),
                    PolicyText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmHouseCancellationPolicies", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_FarmHouseCancellationPolicies_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarmHousePaymentConfigurations",
                columns: table => new
                {
                    FarmHousePaymentConfigurationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MerchantAccountId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CredentialsSecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebhookSecretReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrencyCode = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmHousePaymentConfigurations", x => x.FarmHousePaymentConfigurationId);
                    table.ForeignKey(
                        name: "FK_FarmHousePaymentConfigurations_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarmHousePricing",
                columns: table => new
                {
                    PricingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    PricingKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PricingName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "INR"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmHousePricing", x => x.PricingId);
                    table.ForeignKey(
                        name: "FK_FarmHousePricing_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingActionTokens",
                columns: table => new
                {
                    BookingActionTokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingActionTokens", x => x.BookingActionTokenId);
                    table.ForeignKey(
                        name: "FK_BookingActionTokens_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingActionTokens_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingDocuments",
                columns: table => new
                {
                    BookingDocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "char(64)", unicode: false, maxLength: 64, nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Uploaded"),
                    VerificationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VerifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ReuploadDeadlineUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDocuments", x => x.BookingDocumentId);
                    table.ForeignKey(
                        name: "FK_BookingDocuments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingDocuments_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingDocuments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingEventDates",
                columns: table => new
                {
                    BookingEventDateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    EventDate = table.Column<DateTime>(type: "date", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DailyRateSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingEventDates", x => x.BookingEventDateId);
                    table.ForeignKey(
                        name: "FK_BookingEventDates_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingEventDates_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingStatusHistory",
                columns: table => new
                {
                    BookingStatusHistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    ActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStatusHistory", x => x.BookingStatusHistoryId);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistory_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistory_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistory_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingStayDates",
                columns: table => new
                {
                    BookingStayDateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    StayDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStayDates", x => x.BookingStayDateId);
                    table.ForeignKey(
                        name: "FK_BookingStayDates_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingStayDates_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckoutDiscounts",
                columns: table => new
                {
                    CheckoutDiscountId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EnteredBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutDiscounts", x => x.CheckoutDiscountId);
                    table.ForeignKey(
                        name: "FK_CheckoutDiscounts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckoutDiscounts_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    NotificationOutboxId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<long>(type: "bigint", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.NotificationOutboxId);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "INR"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProviderOrderId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProviderPaymentId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProviderRefundId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PaymentTransactionId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingReceipts",
                columns: table => new
                {
                    ReceiptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "INR"),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingReceipts", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_BookingReceipts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingReceipts_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingReceipts_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "PaymentTransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 993, DateTimeKind.Local).AddTicks(3395));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 993, DateTimeKind.Local).AddTicks(3433));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 993, DateTimeKind.Local).AddTicks(3439));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 993, DateTimeKind.Local).AddTicks(3444));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 993, DateTimeKind.Local).AddTicks(3449));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6460));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6489));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6495));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6500));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6504));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6509));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6513));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6517));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6522));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6525));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 17, 3, 29, 25, 994, DateTimeKind.Local).AddTicks(6530));

            migrationBuilder.CreateIndex(
                name: "IX_BookingActionTokens_BookingId",
                table: "BookingActionTokens",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingActionTokens_ExpiresAtUtc_ConsumedAtUtc",
                table: "BookingActionTokens",
                columns: new[] { "ExpiresAtUtc", "ConsumedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingActionTokens_FarmHouseId_BookingId_Purpose",
                table: "BookingActionTokens",
                columns: new[] { "FarmHouseId", "BookingId", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingActionTokens_TokenHash",
                table: "BookingActionTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingDateReservations_FarmHouseId_ReservationDate_ExpiresAtUtc",
                table: "BookingDateReservations",
                columns: new[] { "FarmHouseId", "ReservationDate", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDateReservations_SelectionId_UserId",
                table: "BookingDateReservations",
                columns: new[] { "SelectionId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDateReservations_UserId",
                table: "BookingDateReservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDocuments_BookingId_CreatedDate",
                table: "BookingDocuments",
                columns: new[] { "BookingId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDocuments_FarmHouseId_BookingId_VerificationStatus",
                table: "BookingDocuments",
                columns: new[] { "FarmHouseId", "BookingId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDocuments_UserId",
                table: "BookingDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingEventDates_BookingId_EventDate",
                table: "BookingEventDates",
                columns: new[] { "BookingId", "EventDate" },
                unique: true,
                filter: "[IsDelete] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BookingEventDates_FarmHouseId_BookingId_EventDate",
                table: "BookingEventDates",
                columns: new[] { "FarmHouseId", "BookingId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingReceipts_BookingId",
                table: "BookingReceipts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReceipts_FarmHouseId_BookingId",
                table: "BookingReceipts",
                columns: new[] { "FarmHouseId", "BookingId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingReceipts_PaymentTransactionId",
                table: "BookingReceipts",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReceipts_ReceiptNumber",
                table: "BookingReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerUserId",
                table: "Bookings",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FarmHouseId_CreatedDate",
                table: "Bookings",
                columns: new[] { "FarmHouseId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FarmHouseId_Status_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "FarmHouseId", "Status", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistory_ActorUserId",
                table: "BookingStatusHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistory_BookingId_NewStatus",
                table: "BookingStatusHistory",
                columns: new[] { "BookingId", "NewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistory_FarmHouseId_BookingId_CreatedDate",
                table: "BookingStatusHistory",
                columns: new[] { "FarmHouseId", "BookingId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStayDates_BookingId_StayDate",
                table: "BookingStayDates",
                columns: new[] { "BookingId", "StayDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStayDates_FarmHouseId_StayDate",
                table: "BookingStayDates",
                columns: new[] { "FarmHouseId", "StayDate" },
                unique: true,
                filter: "[IsDelete] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutDiscounts_BookingId",
                table: "CheckoutDiscounts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutDiscounts_FarmHouseId_BookingId_IsDelete",
                table: "CheckoutDiscounts",
                columns: new[] { "FarmHouseId", "BookingId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_FarmHouseCancellationPolicies_FarmHouseId_IsActive_IsDelete",
                table: "FarmHouseCancellationPolicies",
                columns: new[] { "FarmHouseId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_FarmHouseCancellationPolicies_FarmHouseId_PolicyVersion",
                table: "FarmHouseCancellationPolicies",
                columns: new[] { "FarmHouseId", "PolicyVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmHousePaymentConfigurations_FarmHouseId_IsActive_IsDelete",
                table: "FarmHousePaymentConfigurations",
                columns: new[] { "FarmHouseId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_FarmHousePaymentConfigurations_FarmHouseId_ProviderName_MerchantAccountId",
                table: "FarmHousePaymentConfigurations",
                columns: new[] { "FarmHouseId", "ProviderName", "MerchantAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmHousePricing_FarmHouseId_PricingKey",
                table: "FarmHousePricing",
                columns: new[] { "FarmHouseId", "PricingKey" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsDelete] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_BookingId",
                table: "NotificationOutbox",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_FarmHouseId",
                table: "NotificationOutbox",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_IdempotencyKey",
                table: "NotificationOutbox",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_Status_NextAttemptAtUtc_CreatedDate",
                table: "NotificationOutbox",
                columns: new[] { "Status", "NextAttemptAtUtc", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BookingId",
                table: "PaymentTransactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_FarmHouseId_BookingId_PaymentStatus",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "BookingId", "PaymentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_FarmHouseId_IdempotencyKey",
                table: "PaymentTransactions",
                columns: new[] { "FarmHouseId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingActionTokens");

            migrationBuilder.DropTable(
                name: "BookingDateReservations");

            migrationBuilder.DropTable(
                name: "BookingDocuments");

            migrationBuilder.DropTable(
                name: "BookingEventDates");

            migrationBuilder.DropTable(
                name: "BookingReceipts");

            migrationBuilder.DropTable(
                name: "BookingStatusHistory");

            migrationBuilder.DropTable(
                name: "BookingStayDates");

            migrationBuilder.DropTable(
                name: "CheckoutDiscounts");

            migrationBuilder.DropTable(
                name: "FarmHouseCancellationPolicies");

            migrationBuilder.DropTable(
                name: "FarmHousePaymentConfigurations");

            migrationBuilder.DropTable(
                name: "FarmHousePricing");

            migrationBuilder.DropTable(
                name: "NotificationOutbox");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 861, DateTimeKind.Local).AddTicks(7993));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 861, DateTimeKind.Local).AddTicks(8041));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 861, DateTimeKind.Local).AddTicks(8046));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 861, DateTimeKind.Local).AddTicks(8051));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 861, DateTimeKind.Local).AddTicks(8056));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5096));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5129));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5135));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5139));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5143));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5147));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5152));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5156));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5161));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5165));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 16, 22, 1, 25, 863, DateTimeKind.Local).AddTicks(5169));
        }
    }
}

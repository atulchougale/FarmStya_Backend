using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalUtcAndFarmHouseModuleFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingLogs");

            migrationBuilder.DropTable(
                name: "FarmHousePricing");

            migrationBuilder.DropTable(
                name: "PropertyImage");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Property");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FarmHousePricing",
                columns: table => new
                {
                    PricingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PricingName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmHousePricing", x => x.PricingId);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.PropertyId);
                    table.ForeignKey(
                        name: "FK_Property_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId");
                    table.ForeignKey(
                        name: "FK_Property_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdultCount = table.Column<int>(type: "int", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChildrenCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDays = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsCheckIn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsCheckOut = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDecorationRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDocumentUpload = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEvent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PersonCount = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Booking_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Booking_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "PropertyId");
                    table.ForeignKey(
                        name: "FK_Booking_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyImage",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImage", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_PropertyImage_Property_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Property",
                        principalColumn: "PropertyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingLogs",
                columns: table => new
                {
                    BookingLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingLogs", x => x.BookingLogId);
                    table.ForeignKey(
                        name: "FK_BookingLogs_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Booking_CreatedDate",
                table: "Booking",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_FarmHouseId",
                table: "Booking",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_PropertyId",
                table: "Booking",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Status",
                table: "Booking",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_UserId",
                table: "Booking",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLogs_BookingId",
                table: "BookingLogs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLogs_UserId",
                table: "BookingLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_FarmHouseId",
                table: "Property",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_OwnerId",
                table: "Property",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImage_PropertyId",
                table: "PropertyImage",
                column: "PropertyId");
        }
    }
}

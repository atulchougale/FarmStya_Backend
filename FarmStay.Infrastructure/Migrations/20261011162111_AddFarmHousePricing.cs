using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmHousePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Property_PropertyId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "StatusChangedAt",
                table: "Booking",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Booking",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Booking",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PropertyId",
                table: "Booking",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAmount",
                table: "Booking",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "Booking",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "BookingType",
                table: "Booking",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EventDays",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FarmHouseId",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Booking",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckIn",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckOut",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDocumentUpload",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Booking",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Booking",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PersonCount",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Booking",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "Booking",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FarmHousePricing",
                columns: table => new
                {
                    PricingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PricingName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmHousePricing", x => x.PricingId);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Booking_CreatedDate",
                table: "Booking",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_FarmHouseId",
                table: "Booking",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Status",
                table: "Booking",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_FarmHouses_FarmHouseId",
                table: "Booking",
                column: "FarmHouseId",
                principalTable: "FarmHouses",
                principalColumn: "FarmHouseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Property_PropertyId",
                table: "Booking",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_FarmHouses_FarmHouseId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Property_PropertyId",
                table: "Booking");

            migrationBuilder.DropTable(
                name: "FarmHousePricing");

            migrationBuilder.DropIndex(
                name: "IX_Booking_CreatedDate",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_FarmHouseId",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_Status",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "AdvanceAmount",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "BookingType",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "EventDays",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "FarmHouseId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "IsCheckIn",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "IsCheckOut",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "IsDocumentUpload",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "PersonCount",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "Booking");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "Booking",
                newName: "StatusChangedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Booking",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Booking",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<int>(
                name: "PropertyId",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Booking",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Booking",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(5899));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(5905));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(5907));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(5909));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(5911));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6511));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6516));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6518));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6520));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6522));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6523));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6525));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6526));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6528));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6616));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 10, 3, 17, 9, 2, 799, DateTimeKind.Utc).AddTicks(6618));

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Property_PropertyId",
                table: "Booking",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addaboutus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutUs",
                columns: table => new
                {
                    AboutUsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeroTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeroSubtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HeroImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StoryTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StoryDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FarmHouseId = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyBy = table.Column<int>(type: "int", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutUs", x => x.AboutUsId);
                    table.ForeignKey(
                        name: "FK_AboutUs_FarmHouses_FarmHouseId",
                        column: x => x.FarmHouseId,
                        principalTable: "FarmHouses",
                        principalColumn: "FarmHouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AboutUsFeatures",
                columns: table => new
                {
                    FeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AboutUsId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutUsFeatures", x => x.FeatureId);
                    table.ForeignKey(
                        name: "FK_AboutUsFeatures_AboutUs_AboutUsId",
                        column: x => x.AboutUsId,
                        principalTable: "AboutUs",
                        principalColumn: "AboutUsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2032));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2037));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2040));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2042));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2043));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2610));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2615));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2617));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2619));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2621));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2622));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2624));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2626));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2627));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2629));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 29, 14, 15, 1, 346, DateTimeKind.Utc).AddTicks(2630));

            migrationBuilder.CreateIndex(
                name: "IX_AboutUs_FarmHouseId",
                table: "AboutUs",
                column: "FarmHouseId",
                unique: true,
                filter: "[IsDelete] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AboutUsFeatures_AboutUsId",
                table: "AboutUsFeatures",
                column: "AboutUsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutUsFeatures");

            migrationBuilder.DropTable(
                name: "AboutUs");

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5239));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5256));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5262));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5267));

            migrationBuilder.UpdateData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(5272));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6686));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6697));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6703));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6708));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6711));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6715));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6719));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6727));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6731));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "CreatedDate",
                value: new DateTime(2026, 9, 28, 6, 56, 10, 635, DateTimeKind.Utc).AddTicks(6735));
        }
    }
}

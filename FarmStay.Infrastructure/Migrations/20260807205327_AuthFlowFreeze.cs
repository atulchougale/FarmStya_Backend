using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuthFlowFreeze : Migration
    {
        /// <inheritdoc />
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileOtp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileOtpExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSystemRole",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "FarmHouses");


            migrationBuilder.AddColumn<int>(
                name: "FarmHouseId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "UserRefreshTokens",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "FarmHouseId",
                table: "Property",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                 name: "OwnerUserId",
                 table: "FarmHouses",
                 type: "int",
                 nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FarmHouseId",
                table: "Users",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_FarmHouseId",
                table: "Property",
                column: "FarmHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmHouses_OwnerUserId",
                table: "FarmHouses",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmHouses_Users_OwnerUserId",
                table: "FarmHouses",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Property_FarmHouses_FarmHouseId",
                table: "Property",
                column: "FarmHouseId",
                principalTable: "FarmHouses",
                principalColumn: "FarmHouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_FarmHouses_FarmHouseId",
                table: "Users",
                column: "FarmHouseId",
                principalTable: "FarmHouses",
                principalColumn: "FarmHouseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FarmHouses_Users_OwnerUserId",
                table: "FarmHouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Property_FarmHouses_FarmHouseId",
                table: "Property");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_FarmHouses_FarmHouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FarmHouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Property_FarmHouseId",
                table: "Property");

            migrationBuilder.DropIndex(
                name: "IX_FarmHouses_OwnerUserId",
                table: "FarmHouses");

            migrationBuilder.DropColumn(
                name: "FarmHouseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FarmHouseId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "FarmHouses");

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileOtp",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MobileOtpExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "UserRefreshTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemRole",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "FarmHouses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

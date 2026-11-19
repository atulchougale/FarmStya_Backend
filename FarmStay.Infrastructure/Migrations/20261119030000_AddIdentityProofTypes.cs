using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261119030000_AddIdentityProofTypes")]
public sealed class AddIdentityProofTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("IdentityProofType", "BookingDocuments", type: "nvarchar(40)", maxLength: 40, nullable: true);
        migrationBuilder.AddColumn<string>("OtherDocumentType", "BookingDocuments", type: "nvarchar(100)", maxLength: 100, nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("IdentityProofType", "BookingDocuments");
        migrationBuilder.DropColumn("OtherDocumentType", "BookingDocuments");
    }
}

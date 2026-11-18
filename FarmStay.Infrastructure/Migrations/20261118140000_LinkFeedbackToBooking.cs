using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118140000_LinkFeedbackToBooking")]
public sealed class LinkFeedbackToBooking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>("BookingId", "FeedBacks", nullable: true);
        migrationBuilder.CreateIndex("IX_FeedBacks_BookingId", "FeedBacks", "BookingId",
            unique: true, filter: "[BookingId] IS NOT NULL");
        migrationBuilder.AddForeignKey("FK_FeedBacks_Bookings_BookingId", "FeedBacks", "BookingId", "Bookings",
            principalColumn: "BookingId", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_FeedBacks_Bookings_BookingId", "FeedBacks");
        migrationBuilder.DropIndex("IX_FeedBacks_BookingId", "FeedBacks");
        migrationBuilder.DropColumn("BookingId", "FeedBacks");
    }
}

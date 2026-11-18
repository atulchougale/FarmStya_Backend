using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118230000_AddActualBookingStayTimes")]
public sealed class AddActualBookingStayTimes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>("ActualCheckInAt", "Bookings", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<DateTime>("ActualCheckOutAt", "Bookings", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<DateTime>("CheckoutPaidAt", "Bookings", type: "datetime2", nullable: true);

        migrationBuilder.Sql("""
            UPDATE b
            SET ActualCheckInAt = h.CheckedInAt,
                ActualCheckOutAt = COALESCE(h.CheckedOutAt, p.PaidAt),
                CheckoutPaidAt = p.PaidAt
            FROM dbo.Bookings b
            OUTER APPLY (
                SELECT MIN(CASE WHEN h.NewStatus = 'CheckedIn' THEN h.CreatedDate END) AS CheckedInAt,
                       MIN(CASE WHEN h.NewStatus = 'CheckedOut' THEN h.CreatedDate END) AS CheckedOutAt
                FROM dbo.BookingStatusHistory h
                WHERE h.BookingId = b.BookingId AND h.FarmHouseId = b.FarmHouseId
            ) h
            OUTER APPLY (
                SELECT MIN(p.PaidAt) AS PaidAt
                FROM dbo.PaymentTransactions p
                WHERE p.BookingId = b.BookingId AND p.FarmHouseId = b.FarmHouseId
                  AND p.PaymentType = 'Checkout' AND p.PaymentStatus = 'Success' AND p.IsDelete = 0
            ) p
            WHERE h.CheckedInAt IS NOT NULL OR h.CheckedOutAt IS NOT NULL OR p.PaidAt IS NOT NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ActualCheckInAt", "Bookings");
        migrationBuilder.DropColumn("ActualCheckOutAt", "Bookings");
        migrationBuilder.DropColumn("CheckoutPaidAt", "Bookings");
    }
}

using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261119001000_AddPaymentReceiptReport")]
public sealed class AddPaymentReceiptReport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT dbo.BookingReceipts
                (FarmHouseId, BookingId, PaymentTransactionId, ReceiptNumber, Amount, CurrencyCode, IssuedAt, CreatedBy, CreatedDate)
            SELECT p.FarmHouseId, p.BookingId, p.PaymentTransactionId,
                   CONCAT('RC-', p.FarmHouseId, '-', p.PaymentTransactionId),
                   p.Amount, p.CurrencyCode, COALESCE(p.PaidAt, p.VerifiedAt, p.CreatedDate), p.CreatedBy, GETDATE()
            FROM dbo.PaymentTransactions p
            WHERE p.PaymentStatus = 'Success' AND p.IsDelete = 0 AND p.PaymentType IN ('Advance', 'Checkout')
              AND NOT EXISTS (SELECT 1 FROM dbo.BookingReceipts r WHERE r.PaymentTransactionId = p.PaymentTransactionId);
            """);
        migrationBuilder.Sql("""
            CREATE OR ALTER PROCEDURE dbo.usp_Report_BookingPaymentReceipt
                @FarmHouseId int,
                @BookingId bigint,
                @ReceiptId bigint,
                @CustomerUserId int = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT r.ReceiptNumber, b.BookingReference, f.FarmHouseName, f.Address,
                       f.MobileNumber AS FarmPhone, b.FullName, b.Email, b.MobileNumber,
                       CONVERT(nvarchar(20), b.CheckInDate, 106) AS CheckInDate,
                       CONVERT(nvarchar(20), b.CheckOutDate, 106) AS CheckOutDate,
                       b.BookingType, p.PaymentType, p.PaymentMethod, r.CurrencyCode,
                       CONVERT(nvarchar(30), r.IssuedAt, 120) AS IssuedAt,
                       CONVERT(nvarchar(40), r.Amount) AS Amount,
                       CONVERT(nvarchar(40), b.TotalAmount) AS TotalAmount,
                       CONVERT(nvarchar(40), COALESCE(d.Discount, 0)) AS Discount,
                       CONVERT(nvarchar(40), paid.TotalPaid) AS TotalPaid,
                       CONVERT(nvarchar(40), CASE WHEN b.TotalAmount - COALESCE(d.Discount,0) - paid.TotalPaid > 0
                            THEN b.TotalAmount - COALESCE(d.Discount,0) - paid.TotalPaid ELSE 0 END) AS Remaining,
                       COALESCE(p.ProviderPaymentId, '') AS PaymentReference
                FROM dbo.BookingReceipts r
                JOIN dbo.Bookings b ON b.BookingId=r.BookingId AND b.FarmHouseId=r.FarmHouseId
                JOIN dbo.FarmHouses f ON f.FarmHouseId=r.FarmHouseId
                JOIN dbo.PaymentTransactions p ON p.PaymentTransactionId=r.PaymentTransactionId
                    AND p.FarmHouseId=r.FarmHouseId AND p.BookingId=r.BookingId
                OUTER APPLY (
                    SELECT SUM(x.Amount) TotalPaid FROM dbo.PaymentTransactions x
                    WHERE x.BookingId=b.BookingId AND x.FarmHouseId=b.FarmHouseId
                      AND x.IsDelete=0 AND x.PaymentStatus='Success'
                      AND (x.CreatedDate < p.CreatedDate OR (x.CreatedDate=p.CreatedDate AND x.PaymentTransactionId<=p.PaymentTransactionId))
                ) paid
                OUTER APPLY (
                    SELECT SUM(x.Amount) Discount FROM dbo.CheckoutDiscounts x
                    WHERE x.BookingId=b.BookingId AND x.FarmHouseId=b.FarmHouseId AND x.IsDelete=0
                      AND p.PaymentType='Checkout'
                ) d
                WHERE r.ReceiptId=@ReceiptId AND r.BookingId=@BookingId AND r.FarmHouseId=@FarmHouseId
                  AND b.IsDelete=0 AND p.IsDelete=0 AND p.PaymentStatus='Success'
                  AND (@CustomerUserId IS NULL OR b.CustomerUserId=@CustomerUserId);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Report_BookingPaymentReceipt");
}

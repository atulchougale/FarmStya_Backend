using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class AdminBookingConfirmation(AppDbContext db, IBookingNotifications notifications,
    ILogger<AdminBookingConfirmation> logger) : IAdminBookingConfirmation
{
    public async Task<AdminConfirmationResult> CollectCashAsync(int farmhouse, long bookingId, int actor,
        AdminAdvanceRequest request, CancellationToken ct)
    {
        if (!request.CashReceived || request.Amount < 0 || decimal.Round(request.Amount, 2) != request.Amount)
            return new(400, "Confirm that the full advance has been physically received.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(bookingId, ct);
        var query = db.Bookings.Where(b => b.FarmHouseId == farmhouse && b.BookingId == bookingId && !b.IsDelete);
        var booking = await query.AsNoTracking().SingleOrDefaultAsync(ct);
        if (booking == null) return new(404, "Booking not found.");
        if (request.Amount != booking.AdvanceAmount) return new(409, "The amount must match the full booking advance. Refresh the booking.");
        var payments = db.PaymentTransactions.Where(p => p.FarmHouseId == farmhouse && p.BookingId == bookingId &&
            p.PaymentType == "Advance" && !p.IsDelete);
        var key = $"cash-advance:{bookingId}";
        if (await payments.AnyAsync(p => p.IdempotencyKey == key && p.PaymentStatus == "Success", ct))
            return new(200, "Cash advance was already recorded. No additional payment was created.");
        if (booking.Status != "Approved" || !(booking.CustomerDeadline > DateTime.Now))
            return new(409, "This booking is outside its confirmation window.");
        if (await payments.AnyAsync(p => p.PaymentStatus == "Success", ct))
            return new(409, "The advance has already been paid.");
        if (await payments.AnyAsync(p => p.PaymentMethod == "Online" && p.PaymentStatus == "Pending", ct))
            return new(409, "An online advance order is pending. Verify that payment before collecting cash.");
        var identity = db.BookingDocuments.Where(d => d.FarmHouseId == farmhouse && d.BookingId == bookingId &&
            d.UserId == booking.CustomerUserId && !d.IsDelete && d.DocumentType == "IdentityProof" &&
            (d.VerificationStatus == "Uploaded" || d.VerificationStatus == "Verified"));
        if (!await identity.AnyAsync(ct)) return new(409, "Upload an ID proof before collecting the advance.");
        var now = DateTime.Now;
        var changed = await query.Where(b => b.Status == "Approved" && b.CustomerDeadline > now)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, "Confirmed")
                .SetProperty(b => b.ModifiedDate, now).SetProperty(b => b.ModifiedBy, actor), ct);
        if (changed != 1) return new(409, "The booking changed. Refresh before collecting payment.");
        var payment = new PaymentTransaction {
            FarmHouseId = farmhouse, BookingId = bookingId, PaymentType = "Advance", PaymentMethod = "Cash",
            PaymentStatus = "Success", Amount = booking.AdvanceAmount, CurrencyCode = booking.CurrencyCode ?? "INR",
            IdempotencyKey = key, PaidAt = now, VerifiedAt = now, CreatedBy = actor, CreatedDate = now
        };
        db.PaymentTransactions.Add(payment);
        await db.SaveChangesAsync(ct);
        db.BookingReceipts.Add(new BookingReceipt {
            FarmHouseId = farmhouse, BookingId = bookingId, PaymentTransactionId = payment.PaymentTransactionId,
            ReceiptNumber = $"AD-{farmhouse}-{payment.PaymentTransactionId}", Amount = payment.Amount,
            CurrencyCode = payment.CurrencyCode, IssuedAt = now, CreatedBy = actor
        });
        db.BookingStatusHistories.Add(new BookingStatusHistory {
            BookingId = bookingId, FarmHouseId = farmhouse, ActorUserId = actor, ActorType = "Administration",
            OldStatus = "Approved", NewStatus = "Confirmed", Reason = "Full advance received in cash.", CreatedDate = now
        });
        var confirmed = await query.AsNoTracking().SingleAsync(ct);
        await notifications.StageAsync(confirmed, BookingNotificationEvent.Confirmed, cancellationToken: ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Cash advance {PaymentId} recorded by administration {Actor} for booking {BookingId} in farmhouse {Farmhouse}.",
            payment.PaymentTransactionId, actor, bookingId, farmhouse);
        return new(200, "Cash advance recorded and booking confirmed. Receipt is available.");
    }

    public async Task<AdminConfirmationResult> SendPaymentReminderAsync(int farmhouse, long bookingId, int actor, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(bookingId, ct);
        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b => b.FarmHouseId == farmhouse &&
            b.BookingId == bookingId && !b.IsDelete, ct);
        if (booking == null) return new(404, "Booking not found.");
        if (booking.Status != "Approved" || !(booking.CustomerDeadline > DateTime.Now))
            return new(409, "This booking is outside its confirmation window.");
        if (!await db.BookingDocuments.AnyAsync(d => d.BookingId == bookingId && d.FarmHouseId == farmhouse &&
            !d.IsDelete && d.DocumentType == "IdentityProof" && (d.VerificationStatus == "Uploaded" || d.VerificationStatus == "Verified"), ct))
            return new(409, "Upload an ID proof before requesting the advance.");
        // One manual reminder per fifteen-minute period; retries stage the same outbox jobs.
        var occurrence = DateTime.Now.Ticks / TimeSpan.FromMinutes(15).Ticks;
        await notifications.StageAsync(booking, BookingNotificationEvent.AdvancePaymentRequested,
            cancellationToken: ct, eventInstance: occurrence);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Advance payment reminder queued by administration {Actor} for booking {BookingId}.", actor, bookingId);
        return new(200, "Payment reminder queued. The customer can pay from My Bookings.");
    }

    private async Task LockAsync(long bookingId, CancellationToken ct)
    {
        foreach (var resource in new[] { $"FarmStay:AdvanceOrder:{bookingId}", $"FarmStay:BookingIdentity:{bookingId}" })
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction', @LockTimeout = 15000;
                IF @result < 0 THROW 50013, 'Booking confirmation lock unavailable.', 1;
                """, ct);
    }
}

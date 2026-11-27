using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class BookingDecisionService(AppDbContext db, IBookingNotifications notifications,
    ILogger<BookingDecisionService> logger) : FarmStay.Application.Interfaces.Repositories.IBookingDecisionRepository
{
    public async Task<BookingDecisionOutcome> DecideAsync(int farmHouseId, long bookingId, int actorId,
        string? rejectionReason, CancellationToken cancellationToken)
    {
        var accepted = rejectionReason is null;
        var status = accepted ? "Approved" : "Rejected";
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["BookingId"] = bookingId, ["FarmHouseId"] = farmHouseId, ["ActorUserId"] = actorId,
        });
        logger.LogInformation("Booking decision requested: {Decision}.", status);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var bookingQuery = db.Bookings.Where(b => b.BookingId == bookingId && b.FarmHouseId == farmHouseId && !b.IsDelete);
        var now = DateTime.Now;
        DateTime? customerDeadline = accepted ? now.AddHours(24) : null;
        var updated = await bookingQuery.Where(b => b.Status == "Pending" && b.OwnerDeadline > now)
            .ExecuteUpdateAsync(set => set.SetProperty(b => b.Status, status)
                .SetProperty(b => b.ModifiedBy, actorId)
                .SetProperty(b => b.ModifiedDate, now)
                .SetProperty(b => b.ApprovedAt, accepted ? now : (DateTime?)null)
                .SetProperty(b => b.CustomerDeadline, customerDeadline)
                .SetProperty(b => b.RejectionReason, rejectionReason), cancellationToken);
        if (updated == 0)
        {
            var exists = await bookingQuery.AnyAsync(cancellationToken);
            logger.LogInformation("Booking decision declined. Booking exists: {Exists}.", exists);
            return exists ? BookingDecisionOutcome.Conflict : BookingDecisionOutcome.NotFound;
        }

        if (!accepted)
        {
            var released = await db.BookingStayDates.Where(d => d.BookingId == bookingId &&
                    d.FarmHouseId == farmHouseId && !d.IsDelete)
                .ExecuteUpdateAsync(set => set.SetProperty(d => d.IsDelete, true)
                    .SetProperty(d => d.ModifiedBy, actorId)
                    .SetProperty(d => d.ModifiedDate, now), cancellationToken);
            logger.LogDebug("Staged release of {StayDateCount} stay dates.", released);
        }

        var booking = await bookingQuery.AsNoTracking().SingleAsync(cancellationToken);
        db.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingId = bookingId, FarmHouseId = farmHouseId, OldStatus = "Pending",
            NewStatus = status, ActorType = "Administration", ActorUserId = actorId,
            Reason = rejectionReason, CreatedDate = now
        });
        await notifications.StageAsync(booking,
            accepted ? BookingNotificationEvent.Approved : BookingNotificationEvent.Rejected,
            cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Booking decision committed: {Decision}.", status);
        return BookingDecisionOutcome.Applied;
    }
}

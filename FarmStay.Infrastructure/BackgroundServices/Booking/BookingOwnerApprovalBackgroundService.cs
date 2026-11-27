using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.BackgroundServices;

public sealed class BookingOwnerApprovalBackgroundService(
    IServiceScopeFactory scopeFactory, ILogger<BookingOwnerApprovalBackgroundService> logger)
    : BackgroundService
{
    internal const string ReminderTemplate = "Booking:OwnerApprovalReminder";
    private const string TimeoutReason = "The farmhouse did not approve the booking within 48 hours.";
    private static readonly TimeSpan ReminderInterval = TimeSpan.FromHours(4);

    internal static bool IsReminderWindow(DateTime serverTime)
    {
        var localTime = serverTime.TimeOfDay;
        return localTime >= TimeSpan.FromHours(6) || localTime < TimeSpan.FromMinutes(2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception error) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(error, "Booking owner approval processing failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<IBookingNotifications>();
        var now = DateTime.Now;
        var pending = db.Bookings.AsNoTracking().Where(b => !b.IsDelete && b.Status == "Pending");
        var overdue = await pending.Where(b => b.OwnerDeadline <= now)
            .OrderBy(b => b.OwnerDeadline).Select(b => b.BookingId)
            .Take(100).ToListAsync(cancellationToken);
        foreach (var bookingId in overdue)
        {
            await CancelOverdueAsync(db, notifications, bookingId, now, cancellationToken);
            db.ChangeTracker.Clear();
        }

        if (!IsReminderWindow(now))
            return;

        var firstReminderCutoff = now - ReminderInterval;
        var upcoming = await pending.Where(b => b.CreatedDate <= firstReminderCutoff && b.OwnerDeadline > now)
            .ToListAsync(cancellationToken);
        foreach (var booking in upcoming)
        {
            var elapsedIntervals = (now - booking.CreatedDate).Ticks / ReminderInterval.Ticks;
            var due = DateTime.SpecifyKind(booking.CreatedDate.AddTicks(elapsedIntervals * ReminderInterval.Ticks), DateTimeKind.Local);
            if (!IsReminderWindow(due))
                continue;

            await notifications.StageAsync(booking, BookingNotificationEvent.OwnerApprovalReminder, due, cancellationToken);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CancelOverdueAsync(AppDbContext db, IBookingNotifications notifications,
        long bookingId, DateTime now, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var updated = await db.Bookings.Where(b => b.BookingId == bookingId && !b.IsDelete &&
                b.Status == "Pending" && b.OwnerDeadline <= now)
            .ExecuteUpdateAsync(set => set.SetProperty(b => b.Status, "Cancelled")
                .SetProperty(b => b.CancellationReason, TimeoutReason)
                .SetProperty(b => b.ModifiedDate, now), cancellationToken);
        if (updated == 0)
            return;

        var booking = await db.Bookings.AsNoTracking().SingleAsync(b => b.BookingId == bookingId, cancellationToken);
        await db.BookingStayDates.Where(d => d.BookingId == bookingId &&
                d.FarmHouseId == booking.FarmHouseId && !d.IsDelete)
            .ExecuteUpdateAsync(set => set.SetProperty(d => d.IsDelete, true)
                .SetProperty(d => d.ModifiedDate, now), cancellationToken);
        db.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingId = bookingId,
            FarmHouseId = booking.FarmHouseId,
            OldStatus = "Pending",
            NewStatus = "Cancelled",
            ActorType = "System",
            Reason = TimeoutReason,
            CreatedDate = now
        });
        await notifications.StageAsync(booking, BookingNotificationEvent.OwnerApprovalTimedOut,
            cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Booking {BookingId} cancelled at the owner approval deadline. Stay dates released.", bookingId);
    }
}

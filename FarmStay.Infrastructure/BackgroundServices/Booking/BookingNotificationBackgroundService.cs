using System.Text.Json;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Infrastructure.Data;
using FarmStay.Infrastructure.Services.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.BackgroundServices;

public sealed class BookingNotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingNotificationBackgroundService> logger) : BackgroundService
{
    private const int MaxAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await ProcessNextAsync(stoppingToken))
                    continue;
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Booking notification polling failed ({ErrorType}).", exception.GetType().Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.Now;
        var reminderWindowOpen = BookingOwnerApprovalBackgroundService.IsReminderWindow(now);
        var due = db.NotificationOutbox.Where(job =>
            (job.TemplateKey.StartsWith(BookingNotificationMessage.TemplatePrefix) ||
             job.TemplateKey == "BookingSubmitted") &&
            (reminderWindowOpen || job.TemplateKey != BookingOwnerApprovalBackgroundService.ReminderTemplate) &&
            (job.Status == "Pending" || job.Status == "Processing") &&
            (job.NextAttemptAt == null || job.NextAttemptAt <= now));
        var job = await due.AsNoTracking().OrderBy(row => row.CreatedDate)
            .ThenBy(row => row.NotificationOutboxId).FirstOrDefaultAsync(cancellationToken);
        if (job is null)
            return false;

        var leaseUntil = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Local)
            .AddMinutes(5);
        var claimed = await due.Where(row => row.NotificationOutboxId == job.NotificationOutboxId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(row => row.Status, "Processing")
                .SetProperty(row => row.NextAttemptAt, leaseUntil), cancellationToken);
        if (claimed == 0)
            return true;

        var lease = db.NotificationOutbox.Where(row => row.NotificationOutboxId == job.NotificationOutboxId &&
            row.Status == "Processing" && row.NextAttemptAt == leaseUntil);
        try
        {
            var payload = JsonSerializer.Deserialize<BookingNotificationMessage>(job.PayloadJson)
                ?? throw new InvalidOperationException("Missing booking notification payload.");
            var expectedStatus = job.TemplateKey == "BookingSubmitted" ? "Pending" : payload.ExpectedStatus;
            var isOwnerReminder = job.TemplateKey == BookingOwnerApprovalBackgroundService.ReminderTemplate;
            var checkedAt = DateTime.Now;
            var isCurrent = await db.Bookings.AnyAsync(booking =>
                booking.BookingId == job.BookingId && booking.FarmHouseId == job.FarmHouseId &&
                !booking.IsDelete && booking.Status == expectedStatus &&
                (!isOwnerReminder || booking.OwnerDeadline > checkedAt), cancellationToken);
            if (isOwnerReminder && job.CreatedDate.AddHours(4) <= checkedAt)
                isCurrent = false;
            if (!isCurrent)
            {
                await lease.ExecuteUpdateAsync(update => update.SetProperty(row => row.Status, "Cancelled")
                    .SetProperty(row => row.NextAttemptAt, (DateTime?)null), cancellationToken);
                return true;
            }

            if (job.Channel == "Email")
            {
                await scope.ServiceProvider.GetRequiredService<IEmailService>()
                    .SendEmailAsync(job.Recipient, payload.Subject, payload.Message, isHtml: false);
            }
            else if (job.Channel != "WhatsApp" ||
                !await scope.ServiceProvider.GetRequiredService<IWhatsAppService>()
                    .SendMessageAsync(job.Recipient, payload.Message))
            {
                throw new InvalidOperationException("Notification was rejected.");
            }
        }
        catch (Exception exception)
        {
            var attempts = job.RetryCount + 1;
            var failed = attempts >= MaxAttempts;
            var errorType = exception.GetType().Name;
            var nextAttempt = failed ? (DateTime?)null : DateTime.Now.AddMinutes(1 << (attempts - 1));
            await lease.ExecuteUpdateAsync(update => update
                .SetProperty(row => row.Status, failed ? "Failed" : "Pending")
                .SetProperty(row => row.RetryCount, attempts)
                .SetProperty(row => row.NextAttemptAt, nextAttempt)
                .SetProperty(row => row.LastError, errorType), CancellationToken.None);
            logger.LogWarning("Booking notification {NotificationId} failed on attempt {Attempt} via {Channel}.",
                job.NotificationOutboxId, attempts, job.Channel);
            return true;
        }

        var sentAt = DateTime.Now;
        await lease.ExecuteUpdateAsync(update => update.SetProperty(row => row.Status, "Sent")
            .SetProperty(row => row.SentAt, sentAt)
            .SetProperty(row => row.NextAttemptAt, (DateTime?)null), CancellationToken.None);
        logger.LogInformation("Booking notification {NotificationId} sent via {Channel}.",
            job.NotificationOutboxId, job.Channel);
        return true;
    }
}

using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Json;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.Services.Communication;

internal sealed record BookingNotificationMessage(string Subject, string Message, string ExpectedStatus)
{
    public const string TemplatePrefix = "Booking:";
}

public sealed class BookingNotifications(AppDbContext db, ILogger<BookingNotifications> logger) : IBookingNotifications
{
    public async Task StageAsync(Booking booking, BookingNotificationEvent notification,
        DateTime? reminderDueAt = null, CancellationToken cancellationToken = default,
        long? eventInstance = null, string? detail = null)
    {
        var (customer, administration, status) = notification switch
        {
            BookingNotificationEvent.Submitted => (true, true, "Pending"),
            BookingNotificationEvent.Approved => (true, false, "Approved"),
            BookingNotificationEvent.Confirmed => (true, true, "Confirmed"),
            BookingNotificationEvent.Cancelled => (false, true, "Cancelled"),
            BookingNotificationEvent.OwnerApprovalTimedOut => (true, false, "Cancelled"),
            BookingNotificationEvent.DocumentReuploadRequested => (true, false, "Confirmed"),
            BookingNotificationEvent.Rejected => (true, false, "Rejected"),
            BookingNotificationEvent.OwnerApprovalReminder => (false, true, "Pending"),
            BookingNotificationEvent.CustomerConfirmationReminder => (true, false, "Approved"),
            BookingNotificationEvent.CheckoutPaymentRequested => (true, false, "CheckedIn"),
            BookingNotificationEvent.CheckedOut => (true, false, "CheckedOut"),
            _ => throw new ArgumentOutOfRangeException(nameof(notification))
        };

        var isReminder = notification is BookingNotificationEvent.OwnerApprovalReminder
            or BookingNotificationEvent.CustomerConfirmationReminder;
        if (booking.BookingId <= 0 || booking.IsDelete || booking.Status != status)
            throw new ArgumentException("The booking must have an ID and match the notification status.", nameof(booking));
        if (isReminder != reminderDueAt.HasValue ||
            reminderDueAt is { Kind: not DateTimeKind.Local })
            throw new ArgumentException("Reminders require a scheduled server-time occurrence.", nameof(reminderDueAt));

        var contacts = await db.UserMemberships.AsNoTracking()
            .Where(membership => membership.FarmHouseId == booking.FarmHouseId &&
                membership.IsActive && !membership.IsDeleted &&
                membership.User!.IsActive && !membership.User.IsDeleted &&
                ((customer && membership.UserId == booking.CustomerUserId) ||
                 (administration && membership.Role!.IsActive && !membership.Role.IsDeleted &&
                  (membership.Role.RoleName == "Admin" || membership.Role.RoleName == "Farm Owner" ||
                   membership.Role.RoleName == "FarmOwner"))))
            .Select(membership => new
            {
                membership.User!.Email,
                membership.User.MobileNumber,
                IsAdministration = administration && membership.Role!.IsActive && !membership.Role.IsDeleted &&
                    (membership.Role.RoleName == "Admin" || membership.Role.RoleName == "Farm Owner" ||
                     membership.Role.RoleName == "FarmOwner")
            })
            .Distinct().ToListAsync(cancellationToken);

        var template = BookingNotificationMessage.TemplatePrefix + notification;
        // Keep the identity of jobs staged before the server-clock migration.
        var occurrence = reminderDueAt is { } due ? due.Ticks - TimeZoneInfo.Local.GetUtcOffset(due).Ticks : eventInstance ?? 0;
        var jobs = new Dictionary<string, NotificationOutbox>();
        if (administration && !contacts.Any(contact => contact.IsAdministration &&
            (!string.IsNullOrWhiteSpace(contact.Email) || !string.IsNullOrWhiteSpace(contact.MobileNumber))))
            logger.LogWarning("No active farmhouse administration contact for booking {BookingId} in farmhouse {FarmHouseId}.",
                booking.BookingId, booking.FarmHouseId);

        var farmHouse = await db.FarmHouses.Where(farm => farm.FarmHouseId == booking.FarmHouseId)
            .Select(farm => new { farm.FarmHouseName, farm.DomainName }).SingleAsync(cancellationToken);
        foreach (var contact in contacts.OrderByDescending(contact => contact.IsAdministration))
        {
            var payload = JsonSerializer.Serialize(Compose(booking, farmHouse.FarmHouseName, farmHouse.DomainName,
                notification, status, contact.IsAdministration, detail));
            if (notification != BookingNotificationEvent.OwnerApprovalReminder)
                AddJob("Email", contact.Email, payload);
            AddJob("WhatsApp", contact.MobileNumber, payload);
        }

        var keys = jobs.Keys.ToArray();
        var existingKeys = await db.NotificationOutbox.AsNoTracking()
            .Where(job => keys.Contains(job.IdempotencyKey)).Select(job => job.IdempotencyKey)
            .ToListAsync(cancellationToken);
        var existing = existingKeys.Concat(db.NotificationOutbox.Local.Select(job => job.IdempotencyKey)).ToHashSet();
        db.NotificationOutbox.AddRange(jobs.Values.Where(job => !existing.Contains(job.IdempotencyKey)));

        void AddJob(string channel, string contact, string payload)
        {
            if (string.IsNullOrWhiteSpace(contact))
                return;

            var recipient = contact.Trim();
            var destination = channel == "Email" ? recipient.ToUpperInvariant() : string.Concat(recipient.Where(char.IsDigit));
            if (destination.Length == 0)
                return;

            var identity = FormattableString.Invariant(
                $"{booking.FarmHouseId}|{booking.BookingId}|{template}|{occurrence}|{channel}|{destination}");
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
            jobs.TryAdd(key, new NotificationOutbox
            {
                FarmHouseId = booking.FarmHouseId,
                BookingId = booking.BookingId,
                Channel = channel,
                Recipient = recipient,
                TemplateKey = template,
                PayloadJson = payload,
                IdempotencyKey = key,
                NextAttemptAt = reminderDueAt
            });
        }
    }

    private static BookingNotificationMessage Compose(Booking booking, string farmhouse, string domainName,
        BookingNotificationEvent notification, string status, bool administration, string? detail)
    {
        var (englishTitle, marathiTitle, englishAction, marathiAction) = (notification, administration) switch
        {
            (BookingNotificationEvent.Submitted, true) => ("New booking request", "नवीन बुकिंग विनंती",
                "A customer submitted a booking request. Open Bookings in your dashboard and approve or reject it before the review deadline.",
                "ग्राहकाने बुकिंगची विनंती केली आहे. डॅशबोर्डमध्ये Bookings उघडा आणि दिलेल्या मुदतीपर्यंत विनंती मंजूर करा किंवा नाकारा."),
            (BookingNotificationEvent.Submitted, false) => ("Booking request received", "बुकिंग विनंती प्राप्त झाली",
                "We received your booking request. It is awaiting approval from the farmhouse. Please wait for the decision before making any payment.",
                "तुमची बुकिंग विनंती मिळाली आहे. फार्महाऊसच्या मंजुरीची प्रतीक्षा आहे. निर्णय येईपर्यंत कोणतेही पेमेंट करू नका."),
            (BookingNotificationEvent.Approved, _) => ("Booking request approved", "बुकिंग विनंती मंजूर झाली",
                "Your request was approved. Open your booking to complete the confirmation steps and pay the advance by the specified deadline.",
                "तुमची विनंती मंजूर झाली आहे. बुकिंग उघडा आणि दिलेल्या मुदतीत पुष्टीकरणाची प्रक्रिया पूर्ण करून आगाऊ रक्कम भरा."),
            (BookingNotificationEvent.Confirmed, true) => ("Booking confirmed", "बुकिंग निश्चित झाले",
                "The booking is confirmed. Review the stay details in your dashboard and prepare for the guest’s arrival.",
                "बुकिंग निश्चित झाले आहे. डॅशबोर्डमध्ये मुक्कामाचा तपशील पाहा आणि पाहुण्यांच्या आगमनाची तयारी करा."),
            (BookingNotificationEvent.Confirmed, false) => ("Booking confirmed", "बुकिंग निश्चित झाले",
                "Your booking is confirmed. Bring the original ID documents you uploaded when checking in.",
                "तुमचे बुकिंग निश्चित झाले आहे. चेक-इनच्या वेळी अपलोड केलेली मूळ ओळखपत्रे सोबत आणा."),
            (BookingNotificationEvent.CheckoutPaymentRequested, _) => ("Checkout payment requested", "चेकआउट पेमेंटची विनंती",
                "Open My Bookings and select Pay and Check Out to pay your outstanding balance.",
                "उर्वरित रक्कम भरण्यासाठी My Bookings मध्ये Pay and Check Out निवडा."),
            (BookingNotificationEvent.CheckedOut, _) => ("Thank you for staying with us", "आमच्याकडे मुक्काम केल्याबद्दल धन्यवाद",
                "Your payment and checkout are complete. We would love to hear about your stay.",
                "तुमचे पेमेंट आणि चेकआउट पूर्ण झाले आहे. तुमचा अभिप्राय आम्हाला जरूर कळवा."),
            (BookingNotificationEvent.DocumentReuploadRequested, _) => ("Please re-upload your ID proof", "ओळखपत्र पुन्हा अपलोड करा",
                "Open My Bookings to upload a clear replacement. Your booking remains confirmed. Bring the original ID documents you uploaded when checking in.",
                "स्पष्ट ओळखपत्र पुन्हा अपलोड करण्यासाठी My Bookings उघडा. तुमचे बुकिंग निश्चित आहे. चेक-इनच्या वेळी अपलोड केलेली मूळ ओळखपत्रे सोबत आणा."),
            (BookingNotificationEvent.Cancelled, _) => ("Booking cancelled", "बुकिंग रद्द झाले",
                "This booking has been cancelled. Open the booking in your dashboard to review the updated status.",
                "हे बुकिंग रद्द झाले आहे. अद्ययावत स्थिती पाहण्यासाठी डॅशबोर्डमध्ये बुकिंग उघडा."),
            (BookingNotificationEvent.OwnerApprovalTimedOut, _) => ("Booking request cancelled", "बुकिंग विनंती रद्द झाली",
                "The farmhouse did not approve your request within 48 hours. Your request has been cancelled and the dates have been released. You can submit a new request if dates are available.",
                "फार्महाऊसकडून 48 तासांत मंजुरी मिळाली नाही. तुमची विनंती रद्द झाली असून त्या तारखा मोकळ्या झाल्या आहेत. तारखा उपलब्ध असल्यास तुम्ही पुन्हा विनंती करू शकता."),
            (BookingNotificationEvent.Rejected, _) => ("Booking request declined", "बुकिंग विनंती नाकारली",
                "The farmhouse declined your booking request. Open your booking for details or choose other dates.",
                "फार्महाऊसने तुमची बुकिंग विनंती नाकारली आहे. अधिक माहितीसाठी बुकिंग उघडा किंवा इतर तारखा निवडा."),
            (BookingNotificationEvent.OwnerApprovalReminder, _) => ("Booking review reminder", "बुकिंग निर्णयाची आठवण",
                "A new booking request is still pending. Open Bookings in your dashboard and record your decision before the deadline. Approve or reject the request.",
                "नवीन बुकिंग विनंती अजून प्रलंबित आहे. डॅशबोर्डमध्ये Bookings उघडा आणि मुदतीपूर्वी तुमचा निर्णय नोंदवा. विनंती मंजूर करा किंवा नाकारा."),
            (BookingNotificationEvent.CustomerConfirmationReminder, _) => ("Booking confirmation reminder", "बुकिंग पुष्टीकरणाची आठवण",
                "Your booking is approved and awaits confirmation. Complete the pending steps by the confirmation deadline to keep your booking.",
                "तुमचे बुकिंग मंजूर झाले असून पुष्टीकरण बाकी आहे. बुकिंग कायम ठेवण्यासाठी दिलेल्या मुदतीत उरलेली प्रक्रिया पूर्ण करा."),
            _ => throw new ArgumentOutOfRangeException(nameof(notification))
        };
        var deadline = notification switch
        {
            BookingNotificationEvent.Submitted when administration => booking.OwnerDeadline,
            BookingNotificationEvent.OwnerApprovalReminder => booking.OwnerDeadline,
            BookingNotificationEvent.Approved or BookingNotificationEvent.CustomerConfirmationReminder => booking.CustomerDeadline,
            _ => null
        };
        var englishStay = FormattableString.Invariant($"{booking.CheckInDate:dd MMM yyyy} to {booking.CheckOutDate:dd MMM yyyy}");
        var marathi = CultureInfo.GetCultureInfo("mr-IN");
        var marathiStay = $"{booking.CheckInDate.ToString("dd MMM yyyy", marathi)} ते {booking.CheckOutDate.ToString("dd MMM yyyy", marathi)}";
        var localDeadline = deadline is { } due ? DateTime.SpecifyKind(due, DateTimeKind.Local) : (DateTime?)null;
        var englishDeadline = localDeadline.HasValue ? FormattableString.Invariant($"\nDeadline: {localDeadline:dd MMM yyyy, HH:mm zzz}") : "";
        var marathiDeadline = localDeadline.HasValue ? $"\nअंतिम मुदत: {localDeadline.Value.ToString("dd MMM yyyy, HH:mm zzz", marathi)}" : "";
        var message = $"""
            {englishTitle}
            {englishAction}
            Farmhouse: {farmhouse}
            Booking reference: {booking.BookingReference}
            Stay: {englishStay}{englishDeadline}

            {marathiTitle}
            {marathiAction}
            फार्महाऊस: {farmhouse}
            बुकिंग क्रमांक: {booking.BookingReference}
            मुक्काम: {marathiStay}{marathiDeadline}
            """;
        if (notification == BookingNotificationEvent.Rejected && !string.IsNullOrWhiteSpace(booking.RejectionReason))
            message += $"\n\nReason / कारण: {booking.RejectionReason}";
        if (notification == BookingNotificationEvent.DocumentReuploadRequested && !string.IsNullOrWhiteSpace(detail))
            message += $"\n\nReason / कारण: {detail}";
        var path = notification switch
        {
            BookingNotificationEvent.Approved => $"/my-bookings?reference={Uri.EscapeDataString(booking.BookingReference)}",
            BookingNotificationEvent.DocumentReuploadRequested => $"/my-bookings?reference={Uri.EscapeDataString(booking.BookingReference)}",
            BookingNotificationEvent.Rejected => "/booking",
            BookingNotificationEvent.CheckoutPaymentRequested => $"/my-bookings?reference={Uri.EscapeDataString(booking.BookingReference)}",
            BookingNotificationEvent.CheckedOut => $"/feedback?bookingId={booking.BookingId}",
            _ => null
        };
        var domain = domainName.Trim();
        if (notification is BookingNotificationEvent.Confirmed or BookingNotificationEvent.CheckedOut &&
            Uri.CheckHostName(domain) == UriHostNameType.Dns)
            message += $"\n\nDownload payment receipt / पेमेंट पावती डाउनलोड करा\nhttps://{domain}/my-bookings/{booking.BookingId}?receipts=true";
        if (path != null && Uri.CheckHostName(domain) == UriHostNameType.Dns)
        {
            var label = notification is BookingNotificationEvent.Approved or BookingNotificationEvent.DocumentReuploadRequested
                ? "View My Bookings / माझी बुकिंग पहा"
                : "Start a new booking / नवीन बुकिंग करा";
            message += $"\n\n{label}\nhttps://{domain}{path}";
        }
        return new($"{englishTitle} / {marathiTitle} [{booking.BookingReference}]", message, status);
    }
}

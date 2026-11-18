using BookingEntity = FarmStay.Domain.Entities.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking;

public enum BookingNotificationEvent
{
    Submitted,
    Approved,
    Confirmed,
    Cancelled,
    Rejected,
    OwnerApprovalReminder,
    CustomerConfirmationReminder,
    OwnerApprovalTimedOut,
    DocumentReuploadRequested,
    CheckoutPaymentRequested,
    CheckedOut
}

public interface IBookingNotifications
{
    // Call after obtaining the booking ID and before committing the booking transaction.
    Task StageAsync(BookingEntity booking, BookingNotificationEvent notification,
        DateTime? reminderDueAt = null, CancellationToken cancellationToken = default,
        long? eventInstance = null, string? detail = null);
}

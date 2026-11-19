namespace FarmStay.Application.Interfaces.Services.Booking;

public sealed record AdminAdvanceRequest(decimal Amount, bool CashReceived);
public sealed record AdminConfirmationResult(int Status, string Message);
public interface IAdminBookingConfirmation
{
    Task<AdminConfirmationResult> CollectCashAsync(int farmhouse, long bookingId, int actor,
        AdminAdvanceRequest request, CancellationToken ct);
    Task<AdminConfirmationResult> SendPaymentReminderAsync(int farmhouse, long bookingId, int actor, CancellationToken ct);
}

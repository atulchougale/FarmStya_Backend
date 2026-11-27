using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IAdminBookingConfirmationRepository
{
    Task<AdminConfirmationResult> CollectCashAsync(int farmhouse, long bookingId, int actor,
        AdminAdvanceRequest request, CancellationToken ct);
    Task<AdminConfirmationResult> SendPaymentReminderAsync(int farmhouse, long bookingId, int actor, CancellationToken ct);
}

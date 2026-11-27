using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IBookingStayRepository
{
    Task<StayResult> ExecuteAsync(int tenant, long bookingId, int actor, string action,
        CheckoutRequest? request, CancellationToken ct);
}

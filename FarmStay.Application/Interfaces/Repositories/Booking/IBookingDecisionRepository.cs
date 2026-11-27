using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IBookingDecisionRepository
{
    Task<BookingDecisionOutcome> DecideAsync(int farmHouseId, long bookingId, int actorId,
        string? rejectionReason, CancellationToken cancellationToken);
}

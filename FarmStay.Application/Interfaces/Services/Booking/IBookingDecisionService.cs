namespace FarmStay.Application.Interfaces.Services.Booking;

public enum BookingDecisionOutcome { Applied, NotFound, Conflict }

public interface IBookingDecisionService
{
    Task<BookingDecisionOutcome> DecideAsync(int farmHouseId, long bookingId, int actorId,
        string? rejectionReason, CancellationToken cancellationToken);
}

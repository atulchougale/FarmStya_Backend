using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IAdvancePaymentRepository
{
    Task<(int StatusCode, string Message)> VerifyAsync(int farmHouseId, long bookingId, int customerId, CancellationToken ct);
    Task<AdvanceOrderResult> CreateAsync(int farmHouseId, long bookingId, int customerId, CancellationToken ct);
}

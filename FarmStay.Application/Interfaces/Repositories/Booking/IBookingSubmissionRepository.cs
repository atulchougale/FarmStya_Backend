using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IBookingSubmissionRepository
{
    Task<IReadOnlyList<SubmitBookingResponse>> GetMineAsync(int farmHouseId, int userId,
        CancellationToken cancellationToken);

    Task<BookingSubmissionResult> SubmitForCustomerAsync(int farmHouseId, int customerId, int administrationId,
        string source, SubmitBookingRequest request, CancellationToken cancellationToken);

    Task<BookingSubmissionResult> SubmitAsync(int farmHouseId, int userId,
        SubmitBookingRequest request, CancellationToken cancellationToken);
}

using FarmStay.Application.DTOs.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking;

public record BookingSubmissionResult(int StatusCode, string Message, string? Code = null,
    SubmitBookingResponse? Booking = null, BookingQuoteResponse? Quote = null);

public interface IBookingSubmission
{
    Task<IReadOnlyList<SubmitBookingResponse>> GetMineAsync(int farmHouseId, int userId,
        CancellationToken cancellationToken);

    Task<BookingSubmissionResult> SubmitForCustomerAsync(int farmHouseId, int customerId, int administrationId,
        string source, SubmitBookingRequest request, CancellationToken cancellationToken);

    Task<BookingSubmissionResult> SubmitAsync(int farmHouseId, int userId,
        SubmitBookingRequest request, CancellationToken cancellationToken);
}

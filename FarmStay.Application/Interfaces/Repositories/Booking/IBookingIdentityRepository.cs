using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IBookingIdentityRepository
{
    Task<BookingIdentityDocument?> GetAsync(int farmHouseId, long bookingId, int? ownerId, CancellationToken ct);
    Task<(Stream Content, string ContentType)?> OpenAsync(int farmHouseId, long bookingId, long documentId, CancellationToken ct);
    Task<BookingIdentityDocument?> UploadAsync(int farmHouseId, long bookingId, int ownerId,
        string documentType, string? otherDocumentType, IReadOnlyList<IdentityProofUpload> files, CancellationToken ct);
    Task<BookingIdentityDocument?> UploadForCustomerAsync(int farmHouseId, long bookingId, int actorId,
        string documentType, string? otherDocumentType, IReadOnlyList<IdentityProofUpload> files, CancellationToken ct);
    Task<bool> ReviewAsync(int farmHouseId, long bookingId, long documentId, int reviewerId,
        string? reason, CancellationToken ct);
}

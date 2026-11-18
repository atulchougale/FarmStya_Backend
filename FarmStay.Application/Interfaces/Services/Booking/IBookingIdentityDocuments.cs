namespace FarmStay.Application.Interfaces.Services.Booking;

public sealed record BookingIdentityDocument(long Id, string FileName, string ContentType, string Status, string? Reason);

public interface IBookingIdentityDocuments
{
    Task<BookingIdentityDocument?> GetAsync(int farmHouseId, long bookingId, int? ownerId, CancellationToken ct);
    Task<(Stream Content, string ContentType)?> OpenAsync(int farmHouseId, long bookingId, long documentId, CancellationToken ct);
    Task<BookingIdentityDocument?> UploadAsync(int farmHouseId, long bookingId, int ownerId,
        string fileName, string contentType, byte[] bytes, CancellationToken ct);
    Task<bool> ReviewAsync(int farmHouseId, long bookingId, long documentId, int reviewerId,
        string? reason, CancellationToken ct);
}

namespace FarmStay.Application.Interfaces.Services.Booking;

public sealed record IdentityProofFile(long Id, string FileName, string ContentType);
public sealed record IdentityProofUpload(string FileName, string ContentType, byte[] Bytes);
public sealed record BookingIdentityDocument(long Id, string FileName, string ContentType, string Status, string? Reason,
    string DocumentType, string? OtherDocumentType, IReadOnlyList<IdentityProofFile> Files,
    long UploadedBy, DateTime UploadedAt, bool UploadedByAdministration);

public interface IBookingIdentityDocuments
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

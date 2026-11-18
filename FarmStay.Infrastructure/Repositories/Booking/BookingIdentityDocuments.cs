using System.Security.Cryptography;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class BookingIdentityDocuments(AppDbContext db, IConfiguration configuration,
    IBookingNotifications notifications) : IBookingIdentityDocuments
{
    private const string IdentityProof = "IdentityProof";

    private IQueryable<BookingDocument> Current(int farmHouseId, long bookingId) =>
        db.BookingDocuments.Where(doc => doc.FarmHouseId == farmHouseId && doc.BookingId == bookingId &&
            doc.DocumentType == IdentityProof && !doc.IsDelete && doc.VerificationStatus != "Superseded");

    public async Task<BookingIdentityDocument?> GetAsync(int farmHouseId, long bookingId, int? ownerId, CancellationToken ct)
    {
        var document = await Current(farmHouseId, bookingId).AsNoTracking()
            .Where(doc => doc.Booking != null && !doc.Booking.IsDelete &&
                (!ownerId.HasValue || doc.Booking.CustomerUserId == ownerId.Value))
            .OrderByDescending(doc => doc.BookingDocumentId).FirstOrDefaultAsync(ct);
        return document == null ? null : Map(document);
    }

    public async Task<(Stream Content, string ContentType)?> OpenAsync(int farmHouseId, long bookingId,
        long documentId, CancellationToken ct)
    {
        var document = await Current(farmHouseId, bookingId).AsNoTracking().FirstOrDefaultAsync(doc =>
            doc.BookingDocumentId == documentId && doc.Booking != null && !doc.Booking.IsDelete, ct);
        if (document == null || !Guid.TryParseExact(document.StorageKey, "N", out _))
            return null;
        var path = FilePath(document.StorageKey);
        try
        {
            return (File.OpenRead(path), document.ContentType);
        }
        catch (IOException)
        {
            return null;
        }
    }

    public async Task<BookingIdentityDocument?> UploadAsync(int farmHouseId, long bookingId, int ownerId,
        string fileName, string contentType, byte[] bytes, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(bookingId, ct);
        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b => !b.IsDelete &&
            b.BookingId == bookingId && b.FarmHouseId == farmHouseId && b.CustomerUserId == ownerId, ct);
        if (booking == null)
            return null;

        var previous = await Current(farmHouseId, bookingId)
            .OrderByDescending(doc => doc.BookingDocumentId).FirstOrDefaultAsync(ct);
        var now = DateTime.Now;
        var awaitingConfirmation = booking.Status == "Approved" && booking.CustomerDeadline > now;
        var replacementRequested = booking.Status == "Confirmed" && previous?.VerificationStatus == "ReuploadRequested";
        if (!awaitingConfirmation && !replacementRequested)
            return null;

        var key = Guid.NewGuid().ToString("N");
        var path = FilePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var document = new BookingDocument
        {
            FarmHouseId = farmHouseId, BookingId = bookingId, UserId = ownerId, CreatedBy = ownerId,
            DocumentType = IdentityProof, StorageKey = key, OriginalFileName = fileName,
            ContentType = contentType, FileSizeBytes = bytes.LongLength,
            Sha256Hash = Convert.ToHexString(SHA256.HashData(bytes)), CreatedDate = now
        };
        var commitAttempted = false;
        try
        {
            await File.WriteAllBytesAsync(path, bytes, ct);
            if (previous != null)
            {
                previous.VerificationStatus = "Superseded";
                previous.ModifiedDate = now;
                previous.ModifiedBy = ownerId;
            }
            db.BookingDocuments.Add(document);
            await db.SaveChangesAsync(ct);
            commitAttempted = true;
            await transaction.CommitAsync(ct);
            return Map(document);
        }
        catch
        {
            if (!commitAttempted)
                File.Delete(path);
            throw;
        }
    }

    public async Task<bool> ReviewAsync(int farmHouseId, long bookingId, long documentId, int reviewerId,
        string? reason, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(bookingId, ct);
        var document = await Current(farmHouseId, bookingId).Include(doc => doc.Booking)
            .FirstOrDefaultAsync(doc => doc.BookingDocumentId == documentId && doc.VerificationStatus == "Uploaded" &&
                doc.Booking != null && !doc.Booking.IsDelete && doc.Booking.Status == "Confirmed", ct);
        if (document?.Booking == null)
            return false;

        var now = DateTime.Now;
        document.VerificationStatus = reason == null ? "Verified" : "ReuploadRequested";
        document.VerificationReason = reason;
        document.VerifiedBy = reviewerId;
        document.VerifiedAt = now;
        document.ModifiedBy = reviewerId;
        document.ModifiedDate = now;
        if (reason != null)
            await notifications.StageAsync(document.Booking, BookingNotificationEvent.DocumentReuploadRequested,
                cancellationToken: ct, eventInstance: documentId, detail: reason);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private Task LockAsync(long bookingId, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = {$"FarmStay:BookingIdentity:{bookingId}"},
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @result < 0 THROW 50012, 'Booking document lock unavailable.', 1;
            """, ct);

    private string FilePath(string key) => Path.Combine(
        configuration["BookingDocuments:PrivateRoot"] ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FarmStay", "BookingIdentity"),
        key);

    private static BookingIdentityDocument Map(BookingDocument doc) =>
        new(doc.BookingDocumentId, doc.OriginalFileName, doc.ContentType, doc.VerificationStatus, doc.VerificationReason);
}

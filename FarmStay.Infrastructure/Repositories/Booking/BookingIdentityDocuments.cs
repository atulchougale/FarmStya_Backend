using System.Security.Cryptography;
using FarmStay.Infrastructure.Services.FileUpload;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class BookingIdentityDocuments(AppDbContext db, IConfiguration configuration,
    IBookingNotifications notifications, Microsoft.Extensions.Hosting.IHostEnvironment environment) : FarmStay.Application.Interfaces.Repositories.IBookingIdentityRepository
{
    private const string IdentityProof = "IdentityProof";

    private IQueryable<BookingDocument> Current(int farmHouseId, long bookingId) =>
        db.BookingDocuments.Where(doc => doc.FarmHouseId == farmHouseId && doc.BookingId == bookingId &&
            doc.DocumentType == IdentityProof && !doc.IsDelete && doc.VerificationStatus != "Superseded");

    public async Task<BookingIdentityDocument?> GetAsync(int farmHouseId, long bookingId, int? ownerId, CancellationToken ct)
    {
        var documents = await Current(farmHouseId, bookingId).AsNoTracking()
            .Where(doc => doc.Booking != null && !doc.Booking.IsDelete &&
                (!ownerId.HasValue || doc.Booking.CustomerUserId == ownerId.Value))
            .OrderBy(doc => doc.BookingDocumentId).ToListAsync(ct);
        return documents.Count == 0 ? null : Map(documents);
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
            var stream=File.OpenRead(path);
            try {
                var hash=Convert.ToHexString(await SHA256.HashDataAsync(stream,ct));
                if(stream.Length!=document.FileSizeBytes||!string.Equals(hash,document.Sha256Hash,StringComparison.OrdinalIgnoreCase)){await stream.DisposeAsync();return null;}
                stream.Position=0;return (stream, document.ContentType);
            }catch{await stream.DisposeAsync();throw;}
        }
        catch (IOException)
        {
            return null;
        }
    }

    public async Task<BookingIdentityDocument?> UploadAsync(int farmHouseId, long bookingId, int ownerId,
        string documentType, string? otherDocumentType, IReadOnlyList<IdentityProofUpload> files, CancellationToken ct)
        => await UploadCoreAsync(farmHouseId, bookingId, ownerId, ownerId, documentType, otherDocumentType, files, ct);

    public Task<BookingIdentityDocument?> UploadForCustomerAsync(int farmHouseId, long bookingId, int actorId,
        string documentType, string? otherDocumentType, IReadOnlyList<IdentityProofUpload> files, CancellationToken ct)
        => UploadCoreAsync(farmHouseId, bookingId, null, actorId, documentType, otherDocumentType, files, ct);

    private async Task<BookingIdentityDocument?> UploadCoreAsync(int farmHouseId, long bookingId, int? ownerId, int actorId,
        string documentType, string? otherDocumentType, IReadOnlyList<IdentityProofUpload> files, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(bookingId, ct);
        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b => !b.IsDelete &&
            b.BookingId == bookingId && b.FarmHouseId == farmHouseId && (!ownerId.HasValue || b.CustomerUserId == ownerId), ct);
        if (booking == null)
            return null;

        if (files.Count is < 1 or > 2) return null;
        string[] types=["Aadhaar Card","PAN Card","Passport","Driving Licence","Voter ID","Government-issued Photo ID","Other"];
        if(!types.Contains(documentType)||(documentType=="Other"&&(string.IsNullOrWhiteSpace(otherDocumentType)||otherDocumentType.Trim().Length>100)))return null;
        foreach(var file in files) {
            try {FileSafety.DisplayName(file.FileName);}catch(ArgumentException){return null;}
            if(!FileSafety.ValidProof(file.FileName,file.ContentType,file.Bytes))return null;
        }
        if(files.Select(f=>Convert.ToHexString(SHA256.HashData(f.Bytes))).Distinct().Count()!=files.Count)return null;
        var previous = await Current(farmHouseId, bookingId).ToListAsync(ct);
        // Uploaded identity is immutable until administration explicitly requests replacement.
        if (previous.Count > 0 && !previous.Any(doc => doc.VerificationStatus == "ReuploadRequested")) {
            var hashes=files.Select(f=>Convert.ToHexString(SHA256.HashData(f.Bytes))).OrderBy(h=>h).ToArray();
            var same=previous.Count==files.Count && previous.All(d=>d.IdentityProofType==documentType&&d.OtherDocumentType==(documentType=="Other"?otherDocumentType?.Trim():null))
                && previous.Select(d=>d.Sha256Hash).OrderBy(h=>h).SequenceEqual(hashes,StringComparer.OrdinalIgnoreCase);
            return same ? Map(previous) : null;
        }
        var now = DateTime.Now;
        var awaitingConfirmation = booking.Status == "Approved" && booking.CustomerDeadline > now;
        var replacementRequested = booking.Status == "Confirmed" && previous.Any(doc => doc.VerificationStatus == "ReuploadRequested");
        if (!awaitingConfirmation && !replacementRequested)
            return null;

        var created = files.Select(file => new BookingDocument
        {
            FarmHouseId = farmHouseId, BookingId = bookingId, UserId = booking.CustomerUserId, CreatedBy = actorId,
            DocumentType = IdentityProof, IdentityProofType = documentType,
            OtherDocumentType = documentType == "Other" ? otherDocumentType?.Trim() : null,
            StorageKey = Guid.NewGuid().ToString("N"), OriginalFileName = FileSafety.DisplayName(file.FileName),
            ContentType = file.ContentType, FileSizeBytes = file.Bytes.LongLength,
            Sha256Hash = Convert.ToHexString(SHA256.HashData(file.Bytes)), CreatedDate = now
        }).ToList();
        var commitAttempted = false;
        try
        {
            for (var index = 0; index < created.Count; index++)
            {
                var path = FilePath(created[index].StorageKey);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllBytesAsync(path, files[index].Bytes, ct);
            }
            foreach (var old in previous)
            {
                old.VerificationStatus = "Superseded";
                old.ModifiedDate = now;
                old.ModifiedBy = actorId;
            }
            db.BookingDocuments.AddRange(created);
            await db.SaveChangesAsync(ct);
            commitAttempted = true;
            await transaction.CommitAsync(ct);
            return Map(created);
        }
        catch
        {
            if (!commitAttempted)
                foreach (var doc in created) File.Delete(FilePath(doc.StorageKey));
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
        var group = await Current(farmHouseId, bookingId).ToListAsync(ct);
        foreach (var item in group)
        {
            item.VerificationStatus = reason == null ? "Verified" : "ReuploadRequested";
            item.VerificationReason = reason;
            item.VerifiedBy = reviewerId;
            item.VerifiedAt = now;
            item.ModifiedBy = reviewerId;
            item.ModifiedDate = now;
        }
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

    private string FilePath(string key) => Path.Combine(FileSafety.PrivateRoot(
        configuration["BookingDocuments:PrivateRoot"] ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FarmStay", "BookingIdentity"),
        Path.Combine(environment.ContentRootPath,"wwwroot")), key);

    private static BookingIdentityDocument Map(IReadOnlyList<BookingDocument> documents)
    {
        var doc = documents[0];
        return new(doc.BookingDocumentId, doc.OriginalFileName, doc.ContentType, doc.VerificationStatus,
            doc.VerificationReason, doc.IdentityProofType ?? "ID proof", doc.OtherDocumentType,
            documents.Select(file => new IdentityProofFile(file.BookingDocumentId, file.OriginalFileName, file.ContentType)).ToArray(),
            doc.CreatedBy, doc.CreatedDate, doc.CreatedBy != doc.UserId);
    }
}

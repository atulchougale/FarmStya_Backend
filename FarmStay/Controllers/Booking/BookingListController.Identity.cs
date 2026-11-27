using System.Security.Claims;
using FarmStay.API.Authorization;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Booking;

public sealed partial class BookingListController
{
    [HttpGet("my-bookings/{bookingId:long}/id-proof")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public async Task<IActionResult> MyIdentity(long bookingId, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: false);
        return error ?? Ok(new ApiResponse<BookingIdentityDocument>
        {
            Success = true, Data = await documents.GetAsync(farmHouseId, bookingId, userId, ct)
        });
    }

    [HttpGet("my-bookings/{bookingId:long}/id-proof/{documentId:long}/content")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public async Task<IActionResult> ViewMyIdentity(long bookingId, long documentId, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: false);
        if (error != null) return error;
        var document = await documents.GetAsync(farmHouseId, bookingId, userId, ct);
        if (document == null || !document.Files.Any(file => file.Id == documentId)) return NotFound();
        var file = await documents.OpenAsync(farmHouseId, bookingId, documentId, ct);
        if (file == null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Value.Content, file.Value.ContentType);
    }

    [HttpPost("my-bookings/{bookingId:long}/id-proof")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    [RequestSizeLimit(2 * MaxDocumentBytes + 32768)]
    public async Task<IActionResult> UploadIdentity(long bookingId, [FromForm] List<IFormFile> files,
        [FromForm] string documentType, [FromForm] string? otherDocumentType, CancellationToken ct)
        => await UploadIdentityCore(bookingId, files, documentType, otherDocumentType, false, ct);

    [HttpPost("{bookingId:long}/id-proof")]
    [Authorize(Roles = StaffRoles)]
    [RequestSizeLimit(2 * MaxDocumentBytes + 32768)]
    public Task<IActionResult> UploadCustomerIdentity(long bookingId, [FromForm] List<IFormFile> files,
        [FromForm] string documentType, [FromForm] string? otherDocumentType, CancellationToken ct)
        => UploadIdentityCore(bookingId, files, documentType, otherDocumentType, true, ct);

    private async Task<IActionResult> UploadIdentityCore(long bookingId, List<IFormFile> files,
        string documentType, string? otherDocumentType, bool administration, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration, confirm: administration);
        if (error != null) return error;
        string[] types = ["Aadhaar Card", "PAN Card", "Passport", "Driving Licence", "Voter ID", "Government-issued Photo ID", "Other"];
        if (!types.Contains(documentType) || (documentType == "Other" &&
            (string.IsNullOrWhiteSpace(otherDocumentType) || otherDocumentType.Trim().Length > 100)))
            return BadRequest(new ApiResponse<object> { Message = "Select a document type. For Other, enter a type up to 100 characters." });
        if (files.Count is < 1 or > 2)
            return BadRequest(new ApiResponse<object> { Message = "Upload one or two files for the same document type." });
        var uploads = new List<IdentityProofUpload>();
        foreach (var file in files)
        {
            if (file.Length is <= 0 or > MaxDocumentBytes)
                return BadRequest(new ApiResponse<object> { Message = "Each file must be a PDF, JPG or PNG up to 5 MB." });
            var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
            if (fileName.Length is <= 0 or > 255)
                return BadRequest(new ApiResponse<object> { Message = "The file name is too long or missing." });
            using var buffer = new MemoryStream((int)file.Length);
            await file.CopyToAsync(buffer, ct);
            var bytes = buffer.ToArray();
            var contentType = IdentityContentType(fileName, bytes);
            if (contentType == null || !string.Equals(contentType, file.ContentType, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse<object> { Message = "Choose a valid PDF, JPEG or PNG file." });
            uploads.Add(new(fileName, contentType, bytes));
        }
        if (uploads.Select(file => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(file.Bytes))).Distinct().Count() != uploads.Count)
            return BadRequest(new ApiResponse<object> { Message = "Front and back must be different files." });
        var document = administration
            ? await documents.UploadForCustomerAsync(farmHouseId, bookingId, userId, documentType, otherDocumentType, uploads, ct)
            : await documents.UploadAsync(farmHouseId, bookingId, userId, documentType, otherDocumentType, uploads, ct);
        if (document == null)
            return Conflict(new ApiResponse<object> { Message = "This booking cannot accept an ID upload now." });
        logger.LogInformation("ID proof type {DocumentType} uploaded with {FileCount} files for booking {BookingId} in farmhouse {FarmHouseId}.",
            documentType, uploads.Count, bookingId, farmHouseId);
        return Ok(new ApiResponse<BookingIdentityDocument> { Success = true, Data = document });
    }

    [HttpPost("{bookingId:long}/advance-cash")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> CollectAdvance(long bookingId, AdminAdvanceRequest request, CancellationToken ct)
    {
        var (error, farm, actor) = await GetScope(true, confirm: true);
        if (error != null) return error;
        var result = await confirmation.CollectCashAsync(farm, bookingId, actor, request, ct);
        return StatusCode(result.Status, new ApiResponse<object> { Success = result.Status == 200, Message = result.Message });
    }

    [HttpPost("{bookingId:long}/advance-reminder")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> AdvanceReminder(long bookingId, CancellationToken ct)
    {
        var (error, farm, actor) = await GetScope(true, confirm: true);
        if (error != null) return error;
        var result = await confirmation.SendPaymentReminderAsync(farm, bookingId, actor, ct);
        return StatusCode(result.Status, new ApiResponse<object> { Success = result.Status == 200, Message = result.Message });
    }

    private static string? IdentityContentType(string fileName, ReadOnlySpan<byte> bytes) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" when bytes.StartsWith("%PDF-"u8) => "application/pdf",
            ".jpg" or ".jpeg" when bytes.StartsWith(new byte[] { 255, 216, 255 }) => "image/jpeg",
            ".png" when bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) => "image/png",
            _ => null
        };

    [HttpGet("{bookingId:long}/id-proof")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> Identity(long bookingId, CancellationToken ct)
    {
        var (error, farmHouseId, _) = await GetScope(administration: true);
        return error ?? Ok(new ApiResponse<BookingIdentityDocument>
        {
            Success = true, Data = await documents.GetAsync(farmHouseId, bookingId, null, ct)
        });
    }

    [HttpGet("{bookingId:long}/id-proof/{documentId:long}/content")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> ViewIdentity(long bookingId, long documentId, CancellationToken ct)
    {
        var (error, farmHouseId, _) = await GetScope(administration: true);
        if (error != null) return error;
        var file = await documents.OpenAsync(farmHouseId, bookingId, documentId, ct);
        if (file == null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Value.Content, file.Value.ContentType);
    }

    public sealed record IdentityReviewRequest(bool Verified, string? Reason);

    [HttpPost("{bookingId:long}/id-proof/{documentId:long}/review")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> ReviewIdentity(long bookingId, long documentId, IdentityReviewRequest request,
        CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: true, decide: true);
        if (error != null) return error;
        var reason = request.Verified ? null : request.Reason?.Trim();
        if (!request.Verified && (string.IsNullOrEmpty(reason) || reason.Length > 1000))
            return BadRequest(new ApiResponse<object> { Message = "Enter a re-upload reason of 1 to 1000 characters." });

        if (!await documents.ReviewAsync(farmHouseId, bookingId, documentId, userId, reason, ct))
            return Conflict(new ApiResponse<object> { Message = "This ID proof is no longer awaiting review." });
        logger.LogInformation("ID proof {DocumentId} reviewed for booking {BookingId} by user {ActorId}: verified {Verified}.",
            documentId, bookingId, userId, request.Verified);
        return Ok(new ApiResponse<object> { Success = true, Message = request.Verified ? "ID verified." : "Re-upload requested." });
    }

}

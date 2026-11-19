using System.Security.Claims;
using FarmStay.API.Authorization;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Booking;

[Route("api/booking")]
[ApiController]
[Authorize]
public sealed class BookingListController(IBookingListService service, IBookingDecisionService decisions,
    IBookingIdentityDocuments documents,
    IAdvancePaymentOrders paymentOrders,
    IAdminBookingConfirmation confirmation,
    IBookingStayOperations stays,
    IBookingReceipts receipts,
    ILogger<BookingListController> logger) : ControllerBase
{
    private const string StaffRoles = "Admin,Farm Owner,FarmOwner,Super Admin";
    private const int MaxDocumentBytes = 5 * 1024 * 1024;

    [HttpGet("{bookingId:long}/receipts")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Receipts(long bookingId, CancellationToken ct) => ReadReceipts(bookingId, true, ct);

    [HttpGet("my-bookings/{bookingId:long}/receipts")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> MyReceipts(long bookingId, CancellationToken ct) => ReadReceipts(bookingId, false, ct);

    private async Task<IActionResult> ReadReceipts(long bookingId, bool administration, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null) return error;
        return Ok(new ApiResponse<IReadOnlyList<PaymentReceiptSummary>>
        {
            Success = true, Data = await receipts.ListAsync(farmHouseId, bookingId, administration ? null : userId, ct)
        });
    }

    [HttpGet("{bookingId:long}/receipts/{receiptId:long}/pdf")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> ReceiptPdf(long bookingId, long receiptId, CancellationToken ct) =>
        DownloadReceipt(bookingId, receiptId, true, ct);

    [HttpGet("my-bookings/{bookingId:long}/receipts/{receiptId:long}/pdf")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> MyReceiptPdf(long bookingId, long receiptId, CancellationToken ct) =>
        DownloadReceipt(bookingId, receiptId, false, ct);

    private async Task<IActionResult> DownloadReceipt(long bookingId, long receiptId, bool administration, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null) return error;
        var pdf = await receipts.RenderAsync(farmHouseId, bookingId, receiptId, administration ? null : userId, ct);
        if (pdf == null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        logger.LogInformation("Receipt {ReceiptId} downloaded by user {UserId} for booking {BookingId}.", receiptId, userId, bookingId);
        return File(pdf.Content, "application/pdf", pdf.FileName);
    }

    [HttpPost("{bookingId:long}/check-in")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> CheckIn(long bookingId, CancellationToken ct) =>
        RunStay(bookingId, true, "check-in", null, ct);

    [HttpGet("{bookingId:long}/checkout")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> CheckoutBalance(long bookingId, CancellationToken ct) =>
        RunStay(bookingId, true, "balance", null, ct);

    [HttpPost("{bookingId:long}/checkout")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Checkout(long bookingId, CheckoutRequest request, CancellationToken ct) =>
        RunStay(bookingId, true, "checkout", request, ct);

    [HttpGet("my-bookings/{bookingId:long}/checkout")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> MyCheckout(long bookingId, CancellationToken ct) =>
        RunStay(bookingId, false, "customer-balance", null, ct);

    [HttpPost("my-bookings/{bookingId:long}/checkout-order")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> CheckoutOrder(long bookingId, CancellationToken ct) =>
        RunStay(bookingId, false, "order", null, ct);

    [HttpPost("my-bookings/{bookingId:long}/verify-checkout")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> VerifyCheckout(long bookingId, CancellationToken ct) =>
        RunStay(bookingId, false, "verify", null, ct);

    private async Task<IActionResult> RunStay(long bookingId, bool administration, string action,
        CheckoutRequest? request, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration, decide: administration);
        if (error != null) return error;
        var result = await stays.ExecuteAsync(farmHouseId, bookingId, userId, action, request, ct);
        return StatusCode(result.Status, new ApiResponse<object>
        {
            Success = result.Status is >= 200 and < 300, Message = result.Message, Data = result.Data
        });
    }

    [HttpGet("list")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        ReadList(true, page, pageSize);

    [HttpGet("latest")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Latest([FromQuery] int count = 5) => ReadList(true, 1, count);

    [HttpGet("{bookingId:long}")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Details(long bookingId) => ReadDetails(true, bookingId);

    [HttpGet("my-bookings")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        ReadList(false, page, pageSize);

    [HttpGet("my-bookings/{bookingId:long}")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public Task<IActionResult> MyDetails(long bookingId) => ReadDetails(false, bookingId);

    [HttpPost("my-bookings/{bookingId:long}/advance-order")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public async Task<IActionResult> CreateAdvanceOrder(long bookingId, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: false);
        if (error != null) return error;
        var result = await paymentOrders.CreateAsync(farmHouseId, bookingId, userId, ct);
        return StatusCode(result.StatusCode, new ApiResponse<AdvancePaymentOrder>
        {
            Success = result.Order != null,
            Message = result.Message,
            Data = result.Order
        });
    }

    [HttpPost("my-bookings/{bookingId:long}/verify-advance")]
    [Authorize(Policy = BookingCustomerRequirement.Policy)]
    public async Task<IActionResult> VerifyAdvance(long bookingId, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: false);
        if (error != null) return error;
        var result = await paymentOrders.VerifyAsync(farmHouseId, bookingId, userId, ct);
        return StatusCode(result.StatusCode, new ApiResponse<object>
        {
            Success = result.StatusCode == 200, Message = result.Message
        });
    }

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

    [HttpPost("{bookingId:long}/approve")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Approve(long bookingId, CancellationToken cancellationToken) =>
        Decide(bookingId, null, cancellationToken);

    public sealed record RejectRequest(string? Reason);

    [HttpPost("{bookingId:long}/reject")]
    [Authorize(Roles = StaffRoles)]
    public Task<IActionResult> Reject(long bookingId, RejectRequest request, CancellationToken cancellationToken)
    {
        var reason = request.Reason?.Trim();
        return string.IsNullOrEmpty(reason) || reason.Length > 1000
            ? Task.FromResult<IActionResult>(BadRequest(new ApiResponse<object>
                { Message = "Enter a rejection reason of 1 to 1000 characters." }))
            : Decide(bookingId, reason, cancellationToken);
    }

    private async Task<IActionResult> Decide(long bookingId, string? reason, CancellationToken cancellationToken)
    {
        var (error, farmHouseId, userId) = await GetScope(administration: true, decide: true);
        if (error != null)
        {
            logger.LogWarning("Booking decision access denied for booking {BookingId}.", bookingId);
            return error;
        }

        var outcome = await decisions.DecideAsync(farmHouseId, bookingId, userId, reason, cancellationToken);
        return outcome switch
        {
            BookingDecisionOutcome.Applied => Ok(new ApiResponse<object>
                { Success = true, Message = reason == null ? "Booking approved." : "Booking rejected." }),
            BookingDecisionOutcome.NotFound => NotFound(new ApiResponse<object> { Message = "Booking not found." }),
            _ => Conflict(new ApiResponse<object>
                { Message = "This booking was already decided or its approval deadline has passed." })
        };
    }

    private async Task<IActionResult> ReadList(bool administration, int page, int pageSize)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null)
            return error;
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            return BadRequest(new ApiResponse<object> { Message = "Invalid pagination values." });

        return Ok(new ApiResponse<IReadOnlyList<BookingListResponseDto>>
        {
            Success = true,
            Data = await service.ListAsync(farmHouseId, administration ? null : userId, page, pageSize)
        });
    }

    private async Task<IActionResult> ReadDetails(bool administration, long bookingId)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null)
            return error;

        var booking = await service.FindAsync(farmHouseId, administration ? null : userId, bookingId);
        if (booking != null && administration)
        {
            booking.CanManageConfirmation = await service.CanConfirmAsync(userId, farmHouseId) &&
                ((booking.Status == "Approved" && booking.CustomerDeadline > DateTime.Now) || booking.Status == "Confirmed");
            var identity = await documents.GetAsync(farmHouseId, bookingId, null, HttpContext.RequestAborted);
            booking.HasIdentityDocument = identity != null;
            booking.IdentityDocumentType = identity?.DocumentType;
            booking.OtherIdentityDocumentType = identity?.OtherDocumentType;
            booking.IdentityFileCount = identity?.Files.Count ?? 0;
        }
        if (booking != null && administration && booking.Status is "Pending" or "Confirmed" or "CheckedIn")
        {
            var now = DateTime.Now;
            var authorized = await service.CanDecideAsync(userId, farmHouseId);
            booking.CanDecide = authorized && booking.Status == "Pending" && booking.OwnerDeadline > now;
            booking.CanCheckIn = authorized && booking.Status == "Confirmed" &&
                now.Date >= booking.CheckInDate.Date && now.Date <= booking.CheckOutDate.Date;
            booking.CanCheckout = authorized && booking.Status == "CheckedIn";
        }
        return booking == null
            ? NotFound(new ApiResponse<object> { Message = "Booking not found." })
            : Ok(new ApiResponse<BookingListResponseDto> { Success = true, Data = booking });
    }

    private async Task<(IActionResult? Error, int FarmHouseId, int UserId)> GetScope(bool administration, bool decide = false, bool confirm = false)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
            return (Unauthorized(), 0, 0);
        if (!int.TryParse(Request.Headers["FarmHouseId"].FirstOrDefault(), out var farmHouseId) || farmHouseId <= 0)
            return (BadRequest(new ApiResponse<object> { Message = "A valid farmhouse is required." }), 0, 0);

        var tenantClaim = User.FindFirstValue(JwtClaimNames.FarmHouseId);
        var matchingTenant = int.TryParse(tenantClaim, out var claimedId) && claimedId == farmHouseId;
        if (!matchingTenant && !(administration && User.IsInRole("Super Admin") &&
            await service.IsGlobalAdministratorAsync(userId)))
            return (Forbid(), 0, 0);
        var permitted = confirm ? await service.CanConfirmAsync(userId, farmHouseId)
            : decide ? await service.CanDecideAsync(userId, farmHouseId)
            : await service.CanViewAsync(userId, farmHouseId, administration);
        if (!permitted)
            return (Forbid(), 0, 0);

        return (null, farmHouseId, userId);
    }
}

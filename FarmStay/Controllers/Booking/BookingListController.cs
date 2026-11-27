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
public sealed partial class BookingListController(IBookingListService service, IBookingDecisionService decisions, IBookingActorContext actorContext,
    IBookingIdentityDocuments documents,
    IAdvancePaymentOrders paymentOrders,
    IAdminBookingConfirmation confirmation,
    IBookingStayOperations stays,
    IBookingReceipts receipts,
    ILogger<BookingListController> logger) : ControllerBase
{
    private const string StaffRoles = "Admin,Farm Owner,FarmOwner,Super Admin";
    private const int MaxDocumentBytes = 5 * 1024 * 1024;

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

    [HttpGet("administration-list")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> AdministrationList([FromQuery] AdministrationBookingFilter filter, CancellationToken ct)
    {
        var (error, farmHouseId, userId) = await GetScope(true);
        if (error != null) return error;
        string[] statuses = ["Pending", "Approved", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled", "Rejected", "Completed"];
        if (filter.Page < 1 || filter.PageSize is < 1 or > 100 || (long)(filter.Page - 1) * filter.PageSize > int.MaxValue ||
            filter.Search?.Length > 150 || (!string.IsNullOrEmpty(filter.Status) && !statuses.Contains(filter.Status)) ||
            (!string.IsNullOrEmpty(filter.Source) && filter.Source is not ("Online" or "Phone" or "WalkIn")) ||
            (filter.From.HasValue && filter.To.HasValue && filter.From.Value.Date > filter.To.Value.Date))
            return BadRequest(new ApiResponse<object> { Message = "Invalid booking filters." });
        var result = await service.SearchAdministrationAsync(farmHouseId, filter, ct);
        logger.LogInformation("Administration booking list read by user {UserId} in farmhouse {FarmHouseId}; page {Page}, {Count} matches.", userId, farmHouseId, filter.Page, result.TotalCount);
        return Ok(new ApiResponse<AdministrationBookingList> { Success = true, Data = result });
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
    public Task<IActionResult> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string? status = null)
    {
        if (status != null && status is not ("Confirmed" or "CheckedOut"))
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid booking filter." }));
        return ReadList(false, page, pageSize, status);
    }

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

    private async Task<IActionResult> ReadList(bool administration, int page, int pageSize, string? status = null)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null)
            return error;
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            return BadRequest(new ApiResponse<object> { Message = "Invalid pagination values." });

        return Ok(new ApiResponse<IReadOnlyList<BookingListResponseDto>>
        {
            Success = true,
            Data = await service.ListAsync(farmHouseId, administration ? null : userId, page, pageSize, status)
        });
    }

    private async Task<IActionResult> ReadDetails(bool administration, long bookingId)
    {
        var (error, farmHouseId, userId) = await GetScope(administration);
        if (error != null)
            return error;

        var booking = await service.FindAsync(farmHouseId, administration ? null : userId, bookingId);
        return booking == null
            ? NotFound(new ApiResponse<object> { Message = "Booking not found." })
            : Ok(new ApiResponse<BookingListResponseDto> { Success = true, Data = booking });
    }

    private async Task<(IActionResult? Error, int FarmHouseId, int UserId)> GetScope(bool administration, bool decide = false, bool confirm = false)
    {
        var scope = administration
            ? await actorContext.AdministrationAsync(confirm ? "Bookings.Confirm" : decide ? "Bookings.Decide" : "Bookings.View")
            : await actorContext.CustomerAsync();
        return (null, scope.FarmHouseId, scope.UserId);
    }
}

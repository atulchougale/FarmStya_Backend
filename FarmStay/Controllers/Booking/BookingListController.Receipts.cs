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

}

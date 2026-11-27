using System.Security.Claims;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Booking;

[ApiController]
[Authorize(Policy = FarmStay.API.Authorization.BookingCustomerRequirement.Policy)]
[Route("api/bookings")]
public sealed class BookingsController(IBookingSubmission submission,
    IValidator<SubmitBookingRequest> validator, IBookingActorContext actorContext) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var scope = await actorContext.CustomerAsync(ct: cancellationToken);
        var farmHouseId = scope.FarmHouseId;
        var userId = scope.UserId;

        return Ok(new ApiResponse<IReadOnlyList<SubmitBookingResponse>>
        {
            Success = true,
            Data = await submission.GetMineAsync(farmHouseId, userId, cancellationToken)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Submit([CustomizeValidator(Skip = true)] SubmitBookingRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await actorContext.CustomerAsync(ct: cancellationToken);
        var farmHouseId = scope.FarmHouseId;
        var userId = scope.UserId;

        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return UnprocessableEntity(new ApiResponse<object>
            {
                Message = "Check the booking details.",
                Data = new { code = "INVALID_BOOKING", errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) }
            });

        var result = await submission.SubmitAsync(farmHouseId, userId, request, cancellationToken);
        return StatusCode(result.StatusCode, new ApiResponse<object>
        {
            Success = result.Booking != null,
            Message = result.Message,
            Data = result.Booking != null ? result.Booking : new { code = result.Code, quote = result.Quote }
        });
    }

}

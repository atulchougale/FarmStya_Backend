using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;

namespace FarmStay.API.Authorization;

public sealed class BookingCustomerRequirement : IAuthorizationRequirement
{
    public const string Policy = "BookingCustomer";
}

public sealed class BookingCustomerAuthorizationHandler(
    IHttpContextAccessor accessor, IBookingListService bookings)
    : AuthorizationHandler<BookingCustomerRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        BookingCustomerRequirement requirement)
    {
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !int.TryParse(context.User.FindFirstValue(JwtClaimNames.FarmHouseId), out var tenant) ||
            !int.TryParse(accessor.HttpContext?.Request.Headers["FarmHouseId"].FirstOrDefault(), out var farmHouseId) ||
            farmHouseId <= 0 || tenant != farmHouseId)
            return;

        if (await bookings.CanViewAsync(userId, farmHouseId, administration: false))
            context.Succeed(requirement);
    }
}

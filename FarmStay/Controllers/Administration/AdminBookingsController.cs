using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FarmStay.API.Controllers.Administration;
[ApiController]
[Route("api/administration/bookings")]
[Authorize(Roles="Admin,Farm Owner,FarmOwner,Super Admin")]
public sealed class AdminBookingsController(IAdminBookingCustomers customers, IBookingSubmission submission,
    IBookingCalendarService calendar, IFarmHousePricingService pricing, IValidator<SubmitBookingRequest> validator,
    IUserMembershipRepository memberships, IFarmHouseRepository farmhouses, IPermissionRepository permissions, IBookingListService bookings) : ControllerBase
{
    [HttpGet("customers")]
    public async Task<IActionResult> Customer(string mobileNumber)
    {
        var (error,farm,_) = await Scope("Bookings.Create");
        return error ?? Ok(new { success=true,data=await customers.FindAsync(farm,mobileNumber) });
    }
    [HttpPost("customers")]
    public async Task<IActionResult> Register(AdminCustomerRegistration request)
    {
        var (error,farm,actor)=await Scope("Bookings.Create");
        if(error!=null)return error;
        if(string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length>100 ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.MobileNumber??"","^[0-9]{10}$") ||
            (!string.IsNullOrWhiteSpace(request.Email) && (request.Email.Length>150 ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email))))
            return BadRequest(new {message="Enter a name, valid 10-digit mobile number and valid optional email."});
        var result=await customers.RegisterAsync(farm,actor,request);
        return result.Success?Ok(result):BadRequest(result);
    }
    public record OtpRequest(string Otp);
    [HttpPost("customers/{userId:int}/verify-mobile")]
    public async Task<IActionResult> Verify(int userId,OtpRequest request)
    {
        var(error,farm,_)=await Scope("Bookings.Create");
        if(error!=null)return error;
        if(!System.Text.RegularExpressions.Regex.IsMatch(request.Otp??"","^[0-9]{6}$"))return BadRequest(new{message="Enter a valid 6-digit OTP."});
        var result=await customers.VerifyAsync(farm,userId,request.Otp);
        return result.Success?Ok(result):BadRequest(result);
    }
    [HttpPost("customers/{userId:int}/resend-verification")]
    public async Task<IActionResult> Resend(int userId)
    {
        var(error,farm,_)=await Scope("Bookings.Create");
        if(error!=null)return error;
        var result=await customers.ResendAsync(farm,userId);
        return result.Success?Ok(result):BadRequest(result);
    }
    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(CreateBookingDateReservationRequestDto request)
    {
        var(error,_,_)=await Scope("Bookings.Create");if(error!=null)return error;
        var result=await calendar.CreateReservationAsync(request);
        return result.Success?Ok(result):BadRequest(result);
    }
    [HttpDelete("reserve")]
    public async Task<IActionResult> Release(DateTime fromDate,DateTime? toDate)
    {
        var(error,_,_)=await Scope("Bookings.Create");if(error!=null)return error;
        var result=await calendar.DeleteReservationAsync(new DeleteBookingDateReservationRequestDto{FromDate=fromDate,ToDate=toDate});
        return result.Success?Ok(result):BadRequest(result);
    }
    [HttpGet("reserve/active")]
    public async Task<IActionResult> Active()
    {
        var(error,_,_)=await Scope("Bookings.Create");if(error!=null)return error;
        return Ok(await calendar.GetActiveUserReservationsAsync());
    }
    [HttpPost("quote")]
    public async Task<IActionResult> Quote(BookingQuoteRequest request)
    {
        var(error,_,_)=await Scope("Bookings.Create");if(error!=null)return error;
        var result=await pricing.GetQuoteAsync(request);return result.Success?Ok(result):BadRequest(result);
    }
    public record AdminSubmit(int CustomerUserId,string BookingSource,SubmitBookingRequest Booking);
    [HttpPost]
    public async Task<IActionResult> Submit(AdminSubmit request,CancellationToken ct)
    {
        var(error,farm,actor)=await Scope("Bookings.Create");if(error!=null)return error;
        if(request.CustomerUserId<=0 || request.BookingSource is not ("WalkIn" or "Phone"))
            return BadRequest(new{message="Select Walk-in or Phone enquiry and a verified customer."});
        var check=await validator.ValidateAsync(request.Booking,ct);
        if(!check.IsValid)return UnprocessableEntity(new{message="Check the booking details.",errors=check.Errors.Select(e=>e.ErrorMessage)});
        var result=await submission.SubmitForCustomerAsync(farm,request.CustomerUserId,actor,request.BookingSource,request.Booking,ct);
        return StatusCode(result.StatusCode,new{success=result.Booking!=null,message=result.Message,data=(object?)result.Booking??new{code=result.Code,quote=result.Quote}});
    }
    private async Task<(IActionResult? Error, int Farmhouse, int User)> Scope(string permission)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var user)) return (Unauthorized(), 0, 0);
        if (!int.TryParse(Request.Headers["FarmHouseId"].FirstOrDefault(), out var farmhouse) || farmhouse <= 0)
            return (BadRequest(new { message = "A valid farmhouse is required." }), 0, 0);
        if (await farmhouses.GetByIdAsync(farmhouse) is not { IsActive: true, IsDeleted: false })
            return (NotFound(), 0, 0);
        if (await bookings.IsGlobalAdministratorAsync(user)) return (null, farmhouse, user);
        if (User.FindFirstValue(JwtClaimNames.FarmHouseId) != farmhouse.ToString()) return (Forbid(), 0, 0);
        var membership = await memberships.GetMembershipAsync(user, farmhouse);
        if (membership?.User is not { IsActive: true, IsDeleted: false } ||
            membership.Role is not { IsActive: true, IsDeleted: false, RoleName: "Admin" or "Farm Owner" or "FarmOwner" } ||
            !await permissions.HasPermissionAsync(user, farmhouse, permission)) return (Forbid(), 0, 0);
        return (null, farmhouse, user);
    }
}

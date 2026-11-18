using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Administration;

[ApiController]
[Route("api/administration/website-settings/pricing")]
[Authorize(Roles = "Admin,Farm Owner,FarmOwner,Super Admin")]
public sealed class PricingManagementController(IFarmHousePricingService pricing, IUserMembershipRepository memberships,
    IFarmHouseRepository farmhouses, IPermissionRepository permissions, IBookingListService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var (error, farmhouse, _) = await Scope("Pricing.View");
        return error ?? Ok(await pricing.GetManagementPricingAsync(farmhouse));
    }

    [HttpGet("entries")]
    public async Task<IActionResult> ListEntries()
    {
        var (error, farmhouse, _) = await Scope("Pricing.View");
        return error ?? Ok(new { success = true, data = await pricing.GetEntriesAsync(farmhouse) });
    }

    [HttpGet("entries/{pricingId:int}")]
    public async Task<IActionResult> Entry(int pricingId)
    {
        var (error, farmhouse, _) = await Scope("Pricing.View");
        if (error != null) return error;
        var entry = (await pricing.GetEntriesAsync(farmhouse)).SingleOrDefault(p => p.PricingId == pricingId);
        return entry == null ? NotFound(new { message = "Pricing record not found." }) : Ok(new { success = true, data = entry });
    }

    [HttpPost("entries")]
    public Task<IActionResult> Create(PricingEntryRequest request) => Change(null, request, "Pricing.Create");

    [HttpPut("entries/{pricingId:int}")]
    public Task<IActionResult> Update(int pricingId, PricingEntryRequest request) => Change(pricingId, request, "Pricing.Edit");

    [HttpDelete("entries/{pricingId:int}")]
    public Task<IActionResult> Delete(int pricingId) => Change(pricingId, null, "Pricing.Delete");

    private async Task<IActionResult> Change(int? pricingId, PricingEntryRequest? request, string permission)
    {
        var (error, farmhouse, user) = await Scope(permission);
        if (error != null) return error;
        var result = await pricing.ChangeEntryAsync(farmhouse, user, pricingId, request);
        return StatusCode(result.Status, new { success = result.Status is >= 200 and < 300, message = result.Message, data = result.Entry });
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

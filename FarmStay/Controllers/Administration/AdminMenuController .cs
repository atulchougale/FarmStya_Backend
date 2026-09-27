using FarmStay.API.Authorization;
using FarmStay.Application.DTOs.Administration;
using FarmStay.Application.Interfaces.Services.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Administration
{
    [ApiController]
    [Route("api/admin/menu")]
    [Authorize]
    public class AdminMenuController : ControllerBase
    {
        private readonly IAdminMenuService _adminMenuService;

        public AdminMenuController(
            IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }

        [HttpGet]
        [HasPermission("AdminMenu.Get")]
        public async Task<IActionResult> GetMenu()
        {
            var response = await _adminMenuService.GetMenuAsync();

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
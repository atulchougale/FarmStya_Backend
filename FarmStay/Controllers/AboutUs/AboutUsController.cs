
using FarmStay.API.Authorization;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutUsController : ControllerBase
    {
        private readonly IAboutUsService _aboutUsService;

        public AboutUsController(IAboutUsService aboutUsService)
        {
            _aboutUsService = aboutUsService;
        }

        // 1. Save AboutUs
        [Authorize]
        [HasPermission("AboutUs.Create")]
        [HttpPost("save-aboutus")]
        public async Task<IActionResult> SaveAboutUs( [FromBody] AboutUsRequestDto dto)
        {
            var result = await _aboutUsService.SaveAboutUsAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return MapFailure(
                result.Message,
                result);
        }

        // 2. Get AboutUs
        [Authorize]
        [HasPermission("AboutUs.View")]
        [HttpGet("get-aboutus")]
        public async Task<IActionResult> GetAboutUs()
        {
            var result = await _aboutUsService.GetAboutUsAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return MapFailure(
                result.Message,
                result);
        }

        // 3. Update AboutUs
        [Authorize]
        [HasPermission("AboutUs.Edit")]
        [HttpPut("update-aboutus")]
        public async Task<IActionResult> UpdateAboutUs(
            [FromBody] AboutUsRequestDto dto)
        {
            var result = await _aboutUsService.UpdateAboutUsAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return MapFailure(
                result.Message,
                result);
        }

        // 4. Delete AboutUs
        [Authorize]
        [HasPermission("AboutUs.Delete")]
        [HttpDelete("delete-aboutUs/{aboutUsId:int}")]
        public async Task<IActionResult> DeleteAboutUs(int aboutUsId)
        {
            var result = await _aboutUsService
                .DeleteAboutUsAsync(aboutUsId);

            if (result.Success)
            {
                return Ok(result);
            }

            return MapFailure(
                result.Message,
                result);
        }

        // 5. Common HTTP Error Status Mapping
        private IActionResult MapFailure( string? message, object response)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    response);
            }

            if (message.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (message.Contains(
                "unauthorized",
                StringComparison.OrdinalIgnoreCase))
            {
                if (message.Contains(
                    "not authorized",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        response);
                }

                return Unauthorized(response);
            }

            if (message.Contains(
                "does not belong",
                StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    response);
            }

            if (message.Contains(
                "invalid",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(response);
            }

            if (message.Contains(
                    "something went wrong",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    response);
            }

            // Default for validation and other client errors
            return BadRequest(response);
        }
    }
}
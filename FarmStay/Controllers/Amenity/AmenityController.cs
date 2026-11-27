using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmStay.API.Authorization;

namespace FarmStay.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenityController : ControllerBase
    {
        readonly private IAmenityService _amenityService;

        public AmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        [Authorize]
        [HasPermission("Amenity.Create")]
        [HttpPost("save-amenity")]
        public async Task<IActionResult> SaveAmenity(
            [FromBody] AmenityRequestDto dto)
        {
            var result = await _amenityService.SaveAmenityAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HasPermission("Amenity.View")]
        [HttpGet("get-byid-amenity/{imageId:int}")]
        public async Task<IActionResult> GetGalleryById(int imageId)
        {
            var result = await _amenityService.GetAmenityByIdAsync(imageId);

            return Ok(result);
        }

        [Authorize]
        [HasPermission("Amenity.View")]
        [HttpGet("get-all-amenity")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _amenityService.GetAllAmenityAsync();

            return Ok(result);
        }

        [Authorize]
        [HasPermission("Amenity.Edit")]
        [HttpPut("update-amenity")]
        public async Task<IActionResult> UpdateAsync(
            AmenityRequestDto dto)
        {
            var result = await _amenityService.UpdateAmenityAsync(dto);

            return Ok(result);
        }

        [Authorize]
        [HasPermission("Amenity.Delete")]
        [HttpDelete("delete-amenity/{imageId:int}")]
        public async Task<IActionResult> DeleteAsync(int imageId)
        {
            var result = await _amenityService.DeleteAmenityAsync(imageId);

            return Ok(result);
        }
    }
}
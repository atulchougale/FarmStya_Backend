using FarmStay.Application.DTOs.Admin;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;


namespace FarmStay.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {

        readonly private IGalleryService _galleryService;

        public GalleryController(IGalleryService galleryService)
        {
            _galleryService = galleryService;
        }

        [Authorize]
        [HttpPost("savegallery")]
        public async Task<IActionResult> SaveGallery([FromBody] GalleryRequestDto dto)
        {
            var result = await _galleryService.SaveGalleryAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpGet("get-byid-gallery/{imageId:int}")]
        public async Task<IActionResult> GetGalleryById(int imageId)
        {
            var result = await _galleryService.GetGalleryByIdAsync(imageId);

            return Ok(result);
        }


        [HttpGet("get-all-gallery")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _galleryService.GetAllGalleryAsync();

            return Ok(result);
        }

        [Authorize]
        [HttpPut("update-gallery")]

        public async Task<IActionResult> UpdateAsync(GalleryRequestDto dto)
        {
            var result = await _galleryService.UpdateGalleryAsync(dto);
            return Ok(result);

        }

        [Authorize]
        [HttpDelete("delete-gallery/{imageId:int}")]
        public async Task<IActionResult> DeleteAsync(int imageId)
        {
            var result = await _galleryService.DeleteGalleryAsync(imageId);
            return Ok(result);

        }


        [HttpGet("gallery-categories")]
        public async Task<IActionResult> GetCategoryAsync()
        {
            var result = await _galleryService.GetCategoryListAsync();
            return Ok(result);

        }
    }
}

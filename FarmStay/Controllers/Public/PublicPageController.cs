using FarmStay.Application.Interfaces.Services.Public;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Public
{
    [Route("api/public/[controller]")]
    [ApiController]
    public class PublicPageController : ControllerBase
    {
        private readonly IPublicPageService _publicPageService;

        public PublicPageController(
            IPublicPageService publicPageService)
        {
            _publicPageService = publicPageService;
        }


       
        // AMENITY
       
        [HttpGet("amenities")]
        public async Task<IActionResult> GetAmenities()
        {
            var result =
                await _publicPageService.GetAmenityAsync();

            return Ok(result);
        }


        
        // CAROUSEL
       
        [HttpGet("carousel")]
        public async Task<IActionResult> GetCarousel()
        {
            var result =
                await _publicPageService.GetCarouselAsync();

            return Ok(result);
        }


       
        // TOP 10 FEEDBACK
       
        [HttpGet("top-feedback")]
        public async Task<IActionResult> GetFeedback()
        {
            var result =
                await _publicPageService.GetFeedbackAsync();

            return Ok(result);
        }


       
        // GALLERY
        
        [HttpGet("gallery")]
        public async Task<IActionResult> GetGallery()
        {
            var result =
                await _publicPageService.GetGalleryAsync();

            return Ok(result);
        }
    }
}
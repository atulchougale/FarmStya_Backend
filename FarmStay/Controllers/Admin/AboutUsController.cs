
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        [HttpPost("save-aboutus")]
        public async Task<IActionResult> SaveAboutUs(
                    [FromBody] AboutUsRequestDto dto)
        {
            var result = await _aboutUsService.SaveAboutUsAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    

        [HttpGet("get-aboutus")]
        public async Task<IActionResult> GetAboutUs()
        {
            var result = await _aboutUsService.GetAboutUsAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPut("{aboutUsId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAboutUs(int aboutUsId,[FromBody] AboutUsRequestDto dto)
        {
            var response = await _aboutUsService
                .UpdateAboutUsAsync(aboutUsId, dto);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{aboutUsId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAboutUs(int aboutUsId)
        {
            var result = await _aboutUsService
                .DeleteAboutUsAsync(aboutUsId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
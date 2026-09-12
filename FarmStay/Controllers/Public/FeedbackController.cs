using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Services.Public;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {

        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }



        [Authorize]
        [HttpPost("save-feedback")]

        public async Task<IActionResult> SaveFeedbackAsync(FeedbackRequestDto dto)
        {
            var result = await _feedbackService.SaveFeedbackAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("get-byid-feedback/{feedbackId:int}")]
        public async Task<IActionResult> GetFeedbackById(int feedbackId)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(feedbackId);

            return Ok(result);
        }


        [HttpGet("get-all-feedback")]

        public async Task<IActionResult> GetFeedbackAllAsync()
        {
            var result = await _feedbackService.GetFeedbackAllAsync();

            return Ok(result);
        }


        [Authorize]
        [HttpDelete("delete-feedback/{feedbackId:int}")]
        public async Task<IActionResult> DeleteFeedbackAsync(int feedbackId)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(feedbackId);

            return Ok(result);
        }
    }


}

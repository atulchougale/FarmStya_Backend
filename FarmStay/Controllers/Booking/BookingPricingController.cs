
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Booking
{
    [Route("api/booking-pricing")]
    [ApiController]
    [AllowAnonymous]
    public class BookingPricingController : ControllerBase
    {
        private readonly IFarmHousePricingService _pricingService;
        private readonly ILogger<BookingPricingController> _logger;

        public BookingPricingController(
            IFarmHousePricingService pricingService,
            ILogger<BookingPricingController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivePricing()
        {
            _logger.LogInformation(
                "GetActivePricing API request received.");

            var result =
                await _pricingService.GetActivePricingAsync();

            if (!result.Success)
            {
                _logger.LogWarning(
                    "GetActivePricing API failed. Message: {Message}",
                    result.Message);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FarmStay.Application.Configurations;
using Microsoft.Extensions.Options;

namespace FarmStay.API.Controllers.WhatsApp
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly WhatsAppSettings _whatsAppSettings;

        public WhatsAppWebhookController(IOptions<WhatsAppSettings> whatsAppSettings)
        {
            _whatsAppSettings = whatsAppSettings.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyWebhook(
                [FromQuery(Name = "hub.mode")] string mode,
                [FromQuery(Name = "hub.verify_token")] string verifyToken,
                [FromQuery(Name = "hub.challenge")] string challenge)
        {
            if (mode == "subscribe" && verifyToken == _whatsAppSettings.WebhookVerifyToken)
            {
                return Content(challenge);
            }

            return Unauthorized();
        }
    }
}

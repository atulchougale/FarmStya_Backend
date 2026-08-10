using FarmStay.Application.Interfaces.Services.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Public
{
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IPublicSiteService _publicSiteService;

        public PublicController(IPublicSiteService publicSiteService)
        {
            _publicSiteService = publicSiteService;
        }

        [HttpGet("site")]
        public async Task<IActionResult> GetSite()
        {
            var result = await _publicSiteService.GetSiteAsync();

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
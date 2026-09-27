using FarmStay.Application.DTOs.FileUpload;
using FarmStay.Application.Interfaces.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(524_288_000)] // 500 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)] // 500 MB
        public async Task<IActionResult> UploadFile(
            [FromForm] FileUploadRequestDto request)
        {
            var result = await _fileUploadService.UploadFileAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
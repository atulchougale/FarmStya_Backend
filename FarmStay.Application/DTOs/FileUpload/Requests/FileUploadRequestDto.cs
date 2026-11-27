using Microsoft.AspNetCore.Http;

namespace FarmStay.Application.DTOs.FileUpload
{
    public class FileUploadRequestDto
    {
        public IFormFile File { get; set; } = null!;

        public string Folder { get; set; } = string.Empty;
    }
}
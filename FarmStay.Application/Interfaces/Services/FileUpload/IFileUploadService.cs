using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.FileUpload;

namespace FarmStay.Application.Interfaces.Services.Common
{
    public interface IFileUploadService
    {
        Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(
            FileUploadRequestDto request);
    }
}
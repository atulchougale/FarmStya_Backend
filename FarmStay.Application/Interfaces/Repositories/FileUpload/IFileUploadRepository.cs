using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.FileUpload;

namespace FarmStay.Application.Interfaces.Repositories.FileUpload
{
    public interface IFileUploadRepository
    {
        Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(
            FileUploadRequestDto request);
    }
}
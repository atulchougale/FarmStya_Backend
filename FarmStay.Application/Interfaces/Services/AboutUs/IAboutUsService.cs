using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;

namespace FarmStay.Application.Interfaces.Services.Admin
{
    public interface IAboutUsService
    {
        Task<ApiResponse<AboutUsResponseDto>> SaveAboutUsAsync( AboutUsRequestDto dto);

        Task<ApiResponse<AboutUsResponseDto>> GetAboutUsAsync();

        Task<ApiResponse<AboutUsResponseDto>> UpdateAboutUsAsync(AboutUsRequestDto dto);

        Task<ApiResponse<AboutUsResponseDto>> DeleteAboutUsAsync(int aboutUsId);


    }
}

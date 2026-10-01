using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Services.Admin
{
    public interface IAboutUsService
    {
        Task<ApiResponse<AboutUsResponseDto>> SaveAboutUsAsync(
             AboutUsRequestDto dto);

        Task<ApiResponse<AboutUsResponseDto>> GetAboutUsAsync();

        Task<ApiResponse<AboutUsResponseDto>> UpdateAboutUsAsync( int aboutUsId,AboutUsRequestDto dto);

        Task<ApiResponse<AboutUsResponseDto>> DeleteAboutUsAsync(int aboutUsId);


        //Feature

        // Task<ApiResponse<AboutUsFeatureResponseDto>> SaveFeatureAsync(
        // AboutUsFeatureRequestDto dto);


        //Task<ApiResponse<AboutUsFeatureResponseDto>> GetFeatureByIdAsync(
        //    int featureId);

        //Task<ApiResponse<bool>> DeleteFeatureAsync(
        //    int featureId);
    }
}

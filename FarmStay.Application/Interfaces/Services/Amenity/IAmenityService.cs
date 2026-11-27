using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs;
using FarmStay.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Services.Admin
{
    public interface IAmenityService
    {

        Task<ApiResponse<AmenityResponseDto>> SaveAmenityAsync(AmenityRequestDto dto);

        Task<ApiResponse<AmenityResponseDto>> GetAmenityByIdAsync(int imageId);

        Task<ApiResponse<List<AmenityResponseDto>>> GetAllAmenityAsync();

        Task<ApiResponse<AmenityResponseDto>> UpdateAmenityAsync(AmenityRequestDto dto);

        Task<ApiResponse<AmenityResponseDto>> DeleteAmenityAsync(int imageId);



    }
}

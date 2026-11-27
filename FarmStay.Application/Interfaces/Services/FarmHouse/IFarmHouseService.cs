using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;

namespace FarmStay.Application.Interfaces.Services.Admin
{
    public interface IFarmHouseService
    {
        Task<ApiResponse<FarmHouseResponseDto>> CreateAsync(CreateFarmHouseDto dto);

        Task<ApiResponse<FarmHouseResponseDto>> UpdateAsync(UpdateFarmHouseDto dto);

        Task<ApiResponse<bool>> DeleteAsync(int farmHouseId);

        Task<ApiResponse<FarmHouseResponseDto>> GetByIdAsync(int farmHouseId);

        Task<ApiResponse<FarmHouseResponseDto>> GetByDomainAsync(string domainName);

        Task<ApiResponse<FarmHouseResponseDto>> GetCurrentAsync();

        Task<ApiResponse<List<FarmHouseResponseDto>>> GetAllAsync();
    }
}
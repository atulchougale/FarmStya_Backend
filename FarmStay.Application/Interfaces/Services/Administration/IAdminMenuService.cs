using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Administration;

namespace FarmStay.Application.Interfaces.Services.Administration
{
    public interface IAdminMenuService
    {
        Task<ApiResponse<AdminMenuResponseDto>> GetMenuAsync();
    }
}
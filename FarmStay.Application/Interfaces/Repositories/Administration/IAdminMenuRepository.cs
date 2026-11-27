using FarmStay.Application.DTOs.Administration;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IAdminMenuRepository
    {
        Task<AdminMenuResponseDto> GetMenuAsync(
            int userId,
            int farmHouseId);
    }
}
using FarmStay.Application.Interfaces.Repositories;

namespace FarmStay.Application.Services.Auth
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<bool> HasPermissionAsync(
            int userId,
            int farmHouseId,
            string permissionName)
        {
            if (userId <= 0 ||
                farmHouseId <= 0 ||
                string.IsNullOrWhiteSpace(permissionName))
            {
                return false;
            }

            return await _permissionRepository.HasPermissionAsync(
                userId,
                farmHouseId,
                permissionName.Trim());
        }
    }
}
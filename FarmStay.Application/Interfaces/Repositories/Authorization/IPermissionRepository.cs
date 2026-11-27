namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IPermissionRepository
    {
        Task<bool> HasPermissionAsync(
            int userId,
            int farmHouseId,
            string permissionName);
    }
}
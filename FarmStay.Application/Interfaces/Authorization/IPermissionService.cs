public interface IPermissionService
{
    Task<bool> HasPermissionAsync( int userId, int farmHouseId, string permissionName);
}
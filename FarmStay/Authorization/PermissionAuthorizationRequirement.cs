using Microsoft.AspNetCore.Authorization;

namespace FarmStay.API.Authorization
{
    public sealed class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }

        public PermissionAuthorizationRequirement(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                throw new ArgumentException(
                    "Permission name cannot be null or empty.",
                    nameof(permissionName));
            }

            PermissionName = permissionName.Trim();
        }
    }
}
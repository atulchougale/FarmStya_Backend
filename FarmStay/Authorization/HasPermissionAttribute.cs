using Microsoft.AspNetCore.Authorization;

namespace FarmStay.API.Authorization
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        private const string PolicyPrefix = "Permission:";

        public HasPermissionAttribute(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                throw new ArgumentException(
                    "Permission name cannot be null or empty.",
                    nameof(permissionName));
            }

            Policy = $"{PolicyPrefix}{permissionName.Trim()}";
        }
    }
}
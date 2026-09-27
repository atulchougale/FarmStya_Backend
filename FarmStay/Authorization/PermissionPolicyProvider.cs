using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FarmStay.API.Authorization
{
    public sealed class PermissionPolicyProvider
        : DefaultAuthorizationPolicyProvider
    {
        private const string PolicyPrefix = "Permission:";

        public PermissionPolicyProvider(
            IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(
            string policyName)
        {
            if (!policyName.StartsWith(
                    PolicyPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return await base.GetPolicyAsync(policyName);
            }

            var permissionName =
                policyName[PolicyPrefix.Length..];

            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return null;
            }

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(
                    new PermissionAuthorizationRequirement(
                        permissionName))
                .Build();

            return policy;
        }
    }
}
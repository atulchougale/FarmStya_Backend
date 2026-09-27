using FarmStay.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FarmStay.API.Authorization
{
    public sealed class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionAuthorizationRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userIdClaim = context.User.FindFirst(
                ClaimTypes.NameIdentifier);

            var farmHouseIdClaim = context.User.FindFirst(
                JwtClaimNames.FarmHouseId);

            if (userIdClaim == null ||
                farmHouseIdClaim == null)
            {
                return;
            }

            if (!int.TryParse(userIdClaim.Value, out var userId) ||
                !int.TryParse(farmHouseIdClaim.Value, out var farmHouseId))
            {
                return;
            }

            var hasPermission =
                await _permissionService.HasPermissionAsync(
                    userId,
                    farmHouseId,
                    requirement.PermissionName);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
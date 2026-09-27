using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Administration;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FarmStay.Application.Services.Administration
{
    public class AdminMenuService : IAdminMenuService
    {
        private readonly IAdminMenuRepository _adminMenuRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly ILogger<AdminMenuService> _logger;

        public AdminMenuService(
            IAdminMenuRepository adminMenuRepository,
            IHttpContextAccessor httpContextAccessor,
            IFarmHouseRepository farmHouseRepository,
            IUserMembershipRepository userMembershipRepository,
            ILogger<AdminMenuService> logger)
        {
            _adminMenuRepository = adminMenuRepository;
            _httpContextAccessor = httpContextAccessor;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<AdminMenuResponseDto>> GetMenuAsync()
        {
            _logger.LogInformation(
                "Admin menu request received.");

            // Step 1
            // Get UserId from JWT

            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (!int.TryParse(userIdClaim, out var userId) ||
                userId <= 0)
            {
                _logger.LogWarning(
                    "Unauthorized admin menu request.");

                return new ApiResponse<AdminMenuResponseDto>
                {
                    Success = false,
                    Message = "Unauthorized."
                };
            }

            // Step 2
            // Get FarmHouseId from Header

            var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                .Request
                .Headers["FarmHouseId"]
                .FirstOrDefault();

            if (!int.TryParse(farmHouseIdHeader, out var farmHouseId) ||
                farmHouseId <= 0)
            {
                _logger.LogWarning(
                    "FarmHouseId header is missing or invalid. UserId: {UserId}",
                    userId);

                return new ApiResponse<AdminMenuResponseDto>
                {
                    Success = false,
                    Message = "Invalid FarmHouse."
                };
            }

            // Step 3
            // Validate FarmHouse

            var farmHouse =
                await _farmHouseRepository.GetByIdAsync(farmHouseId);

            if (farmHouse == null)
            {
                _logger.LogWarning(
                    "Invalid FarmHouse. FarmHouseId: {FarmHouseId}, UserId: {UserId}",
                    farmHouseId,
                    userId);

                return new ApiResponse<AdminMenuResponseDto>
                {
                    Success = false,
                    Message = "Invalid FarmHouse."
                };
            }

            // Step 4
            // Validate User belongs to FarmHouse

            var membership =
                await _userMembershipRepository.GetMembershipAsync(
                    userId,
                    farmHouseId);

            if (membership == null)
            {
                _logger.LogWarning(
                    "User is not an active member of FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                    userId,
                    farmHouseId);

                return new ApiResponse<AdminMenuResponseDto>
                {
                    Success = false,
                    Message = "You are not authorized to access this FarmHouse."
                };
            }

            // Step 5
            // Get modules and submenus based on
            // FarmHouse + User Role

            var menu =
                await _adminMenuRepository.GetMenuAsync(
                    userId,
                    farmHouseId);

            _logger.LogInformation(
                "Admin menu retrieved successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                userId,
                farmHouseId);

            return new ApiResponse<AdminMenuResponseDto>
            {
                Success = true,
                Message = "Admin menu retrieved successfully.",
                Data = menu
            };
        }
    }
}
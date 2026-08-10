
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Admin
{
    public class FarmHouseService : IFarmHouseService
    {
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<FarmHouseService> _logger;

        public FarmHouseService(
            IFarmHouseRepository farmHouseRepository,
            IUnitOfWork unitOfWork,
            ITenantResolver tenantResolver,
            ILogger<FarmHouseService> logger)
        {
            _farmHouseRepository = farmHouseRepository;
            _unitOfWork = unitOfWork;
            _tenantResolver = tenantResolver;
            _logger = logger;
        }

        public async Task<ApiResponse<FarmHouseResponseDto>> CreateAsync(CreateFarmHouseDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Creating FarmHouse with domain: {Domain}",
                    dto.DomainName);

                var existingFarmHouse =
                    await _farmHouseRepository.GetByDomainAsync(dto.DomainName);

                if (existingFarmHouse != null)
                {
                    _logger.LogWarning(
                        "FarmHouse already exists with domain: {Domain}",
                        dto.DomainName);

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "FarmHouse with the same domain already exists."
                    };
                }

                var farmHouse = new FarmHouse();

                MapToEntity(farmHouse, dto);

                farmHouse.CreatedDate = DateTime.UtcNow;
                farmHouse.IsDeleted = false;

                await _farmHouseRepository.AddAsync(farmHouse);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "FarmHouse created successfully. FarmHouseId: {FarmHouseId}",
                    farmHouse.FarmHouseId);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = true,
                    Message = "FarmHouse created successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating FarmHouse. Domain: {Domain}",
                    dto.DomainName);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while creating FarmHouse."
                };
            }
        }

        public async Task<ApiResponse<FarmHouseResponseDto>> UpdateAsync(UpdateFarmHouseDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Updating FarmHouse. FarmHouseId: {FarmHouseId}",
                    dto.FarmHouseId);

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(dto.FarmHouseId);

                if (farmHouse == null || farmHouse.IsDeleted)
                {
                    _logger.LogWarning(
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        dto.FarmHouseId);

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "FarmHouse not found."
                    };
                }

                var existingDomain =
                    await _farmHouseRepository.GetByDomainAsync(dto.DomainName);

                if (existingDomain != null &&
                    existingDomain.FarmHouseId != dto.FarmHouseId)
                {
                    _logger.LogWarning(
                        "Duplicate domain found while updating FarmHouse. Domain: {Domain}",
                        dto.DomainName);

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "Another FarmHouse already exists with the same domain."
                    };
                }

                MapToEntity(farmHouse, dto);

                farmHouse.ModifiedDate = DateTime.UtcNow;

                await _farmHouseRepository.UpdateAsync(farmHouse);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "FarmHouse updated successfully. FarmHouseId: {FarmHouseId}",
                    farmHouse.FarmHouseId);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = true,
                    Message = "FarmHouse updated successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while updating FarmHouse. FarmHouseId: {FarmHouseId}",
                    dto.FarmHouseId);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while updating FarmHouse."
                };
            }
        }

        public async Task<ApiResponse<FarmHouseResponseDto>> GetByIdAsync(int farmHouseId)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching FarmHouse. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null || farmHouse.IsDeleted)
                {
                    _logger.LogWarning(
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "FarmHouse not found."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse fetched successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = true,
                    Message = "FarmHouse fetched successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching FarmHouse. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while fetching FarmHouse."
                };
            }
        }

        public async Task<ApiResponse<List<FarmHouseResponseDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all FarmHouses.");

                var farmHouses = await _farmHouseRepository.GetAllAsync();

                var response = farmHouses
                    .Where(x => !x.IsDeleted)
                    .Select(MapToResponseDto)
                    .ToList();

                _logger.LogInformation(
                    "Successfully fetched {Count} FarmHouse(s).",
                    response.Count);

                return new ApiResponse<List<FarmHouseResponseDto>>
                {
                    Success = true,
                    Message = "FarmHouse list fetched successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching FarmHouse list.");

                return new ApiResponse<List<FarmHouseResponseDto>>
                {
                    Success = false,
                    Message = "An unexpected error occurred while fetching FarmHouse list."
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int farmHouseId)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting FarmHouse. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null || farmHouse.IsDeleted)
                {
                    _logger.LogWarning(
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "FarmHouse not found.",
                        Data = false
                    };
                }

                farmHouse.IsDeleted = true;
                farmHouse.IsActive = false;
                farmHouse.ModifiedDate = DateTime.UtcNow;

                await _farmHouseRepository.UpdateAsync(farmHouse);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "FarmHouse deleted successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "FarmHouse deleted successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while deleting FarmHouse. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while deleting FarmHouse.",
                    Data = false
                };
            }
        }
        public async Task<ApiResponse<FarmHouseResponseDto>> GetByDomainAsync(string domainName)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching FarmHouse by domain: {Domain}",
                    domainName);

                var farmHouse = await _farmHouseRepository.GetByDomainAsync(domainName);

                if (farmHouse == null || farmHouse.IsDeleted)
                {
                    _logger.LogWarning(
                        "FarmHouse not found for domain: {Domain}",
                        domainName);

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "FarmHouse not found."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse fetched successfully for domain: {Domain}",
                    domainName);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = true,
                    Message = "FarmHouse fetched successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching FarmHouse. Domain: {Domain}",
                    domainName);

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while fetching FarmHouse."
                };
            }
        }

        public async Task<ApiResponse<FarmHouseResponseDto>> GetCurrentAsync()
        {
            try
            {
                _logger.LogInformation("Fetching current FarmHouse.");

                var farmHouse = await _farmHouseRepository.GetCurrentAsync();

                if (farmHouse == null)
                {
                    _logger.LogWarning("Current FarmHouse not found.");

                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "Current FarmHouse not found."
                    };
                }

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = true,
                    Message = "Current FarmHouse fetched successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching current FarmHouse.");

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                };
            }
        }


        private static void MapToEntity(FarmHouse entity, CreateFarmHouseDto dto)
        {
            entity.FarmHouseName = dto.FarmHouseName;
            entity.DomainName = dto.DomainName;
            entity.SubDomain = dto.SubDomain;
            entity.IsPrimaryDomain = dto.IsPrimaryDomain;
            entity.TagLine = dto.TagLine;
            entity.Description = dto.Description;
            entity.Address = dto.Address;
            entity.Village = dto.Village;
            entity.Taluka = dto.Taluka;
            entity.District = dto.District;
            entity.State = dto.State;
            entity.Pincode = dto.Pincode;
            entity.ContactPersonName = dto.ContactPersonName;
            entity.OwnerUserId  = dto.OwnerUserId ;
            entity.MobileNumber = dto.MobileNumber;
            entity.AlternateMobileNumber = dto.AlternateMobileNumber;
            entity.Email = dto.Email;
            entity.LogoUrl = dto.LogoUrl;
            entity.CoverImageUrl = dto.CoverImageUrl;
            entity.GoogleMapUrl = dto.GoogleMapUrl;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.IsWebsitePublished = dto.IsWebsitePublished;
            entity.IsActive = dto.IsActive;
        }

        private static FarmHouseResponseDto MapToResponseDto(FarmHouse farmHouse)
        {
            return new FarmHouseResponseDto
            {
                FarmHouseId = farmHouse.FarmHouseId,
                FarmHouseName = farmHouse.FarmHouseName,
                DomainName = farmHouse.DomainName,
                SubDomain = farmHouse.SubDomain,
                IsPrimaryDomain = farmHouse.IsPrimaryDomain,
                TagLine = farmHouse.TagLine,
                Description = farmHouse.Description,
                Address = farmHouse.Address,
                Village = farmHouse.Village,
                Taluka = farmHouse.Taluka,
                District = farmHouse.District,
                State = farmHouse.State,
                Pincode = farmHouse.Pincode,
                ContactPersonName = farmHouse.ContactPersonName,
                OwnerUserId  = farmHouse.OwnerUserId ,
                MobileNumber = farmHouse.MobileNumber,
                AlternateMobileNumber = farmHouse.AlternateMobileNumber,
                Email = farmHouse.Email,
                LogoUrl = farmHouse.LogoUrl,
                CoverImageUrl = farmHouse.CoverImageUrl,
                GoogleMapUrl = farmHouse.GoogleMapUrl,
                Latitude = farmHouse.Latitude,
                Longitude = farmHouse.Longitude,
                IsWebsitePublished = farmHouse.IsWebsitePublished,
                SubscriptionPlanId = farmHouse.SubscriptionPlanId,
                SubscriptionStartDate = farmHouse.SubscriptionStartDate,
                SubscriptionEndDate = farmHouse.SubscriptionEndDate,
                IsSubscriptionActive = farmHouse.IsSubscriptionActive,
                IsActive = farmHouse.IsActive,
                CreatedDate = farmHouse.CreatedDate,
                ModifiedDate = farmHouse.ModifiedDate
            };
        }

        private static void MapToEntity(FarmHouse entity, UpdateFarmHouseDto dto)
        {
            entity.FarmHouseName = dto.FarmHouseName;
            entity.DomainName = dto.DomainName;
            entity.SubDomain = dto.SubDomain;
            entity.IsPrimaryDomain = dto.IsPrimaryDomain;
            entity.TagLine = dto.TagLine;
            entity.Description = dto.Description;
            entity.Address = dto.Address;
            entity.Village = dto.Village;
            entity.Taluka = dto.Taluka;
            entity.District = dto.District;
            entity.State = dto.State;
            entity.Pincode = dto.Pincode;
            entity.ContactPersonName = dto.ContactPersonName;
            entity.OwnerUserId  = dto.OwnerUserId ;
            entity.MobileNumber = dto.MobileNumber;
            entity.AlternateMobileNumber = dto.AlternateMobileNumber;
            entity.Email = dto.Email;
            entity.LogoUrl = dto.LogoUrl;
            entity.CoverImageUrl = dto.CoverImageUrl;
            entity.GoogleMapUrl = dto.GoogleMapUrl;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.IsWebsitePublished = dto.IsWebsitePublished;
            entity.IsActive = dto.IsActive;
        }
    }
}

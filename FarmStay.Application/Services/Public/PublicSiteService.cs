using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Public
{
    public class PublicSiteService : IPublicSiteService
    {
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<PublicSiteService> _logger;

    public PublicSiteService(
        IFarmHouseRepository farmHouseRepository,
        ITenantResolver tenantResolver,
        ILogger<PublicSiteService> logger)
        {
            _farmHouseRepository = farmHouseRepository;
            _tenantResolver = tenantResolver;
            _logger = logger;
        }

        public async Task<ApiResponse<PublicSiteDto>> GetSiteAsync()
        {
            try
            {
                _logger.LogInformation("Fetching public site information.");

                var domain = _tenantResolver.GetCurrentDomain();

                if (string.IsNullOrWhiteSpace(domain))
                {
                    _logger.LogWarning("Current domain could not be resolved.");

                    return new ApiResponse<PublicSiteDto>
                    {
                        Success = false,
                        Message = "Domain not found."
                    };
                }

                _logger.LogInformation(
                    "Current domain resolved: {Domain}",
                    domain);

                var farmHouse = await _farmHouseRepository.GetByDomainAsync(domain);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Public site not found for domain {Domain}",
                        domain);

                    return new ApiResponse<PublicSiteDto>
                    {
                        Success = false,
                        Message = "Site not found."
                    };
                }

                _logger.LogInformation(
                    "Public site loaded successfully. FarmHouseId: {FarmHouseId}",
                    farmHouse.FarmHouseId);

                return new ApiResponse<PublicSiteDto>
                {
                    Success = true,
                    Message = "Public site loaded successfully.",
                    Data = MapToDto(farmHouse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while loading the public site.");

                return new ApiResponse<PublicSiteDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while loading the public site."
                };
            }
        }

        private static PublicSiteDto MapToDto(FarmHouse farmHouse)
        {
            return new PublicSiteDto
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
                OwnerName = farmHouse.OwnerUser != null
                    ? farmHouse.OwnerUser.FullName
                    : string.Empty,
                MobileNumber = farmHouse.MobileNumber,
                AlternateMobileNumber = farmHouse.AlternateMobileNumber,
                Email = farmHouse.Email,
                LogoUrl = farmHouse.LogoUrl,
                CoverImageUrl = farmHouse.CoverImageUrl,
                GoogleMapUrl = farmHouse.GoogleMapUrl,
                Latitude = farmHouse.Latitude,
                Longitude = farmHouse.Longitude
            };
        }
    }

}

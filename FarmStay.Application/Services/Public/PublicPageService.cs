
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Public
{
    public class PublicPageService : IPublicPageService
    {
        private readonly IPublicPageRepository _publicPageRepository;
        private readonly ILogger<PublicPageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IAboutUsRepository _aboutUsRepository;

        public PublicPageService(
            IPublicPageRepository publicPageRepository,
            ILogger<PublicPageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFarmHouseRepository farmHouseRepository,
            IAboutUsRepository aboutUsRepository)
        {
            _publicPageRepository = publicPageRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _farmHouseRepository = farmHouseRepository;
            _aboutUsRepository = aboutUsRepository;
        }

        // ==========================================
        // AMENITY
        // ==========================================
        public async Task<ApiResponse<List<AmenityResponseDto>>> GetAmenityAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get all amenities request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Get Amenities
                var amenities = await _publicPageRepository
                    .GetAmenitiesAsync(farmHouseId);

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = true,
                    Message = "Amenities fetched successfully.",
                    Data = amenities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting amenities.");

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = false,
                    Message = "Something went wrong."
                };
            }
        }


        // ==========================================
        // CAROUSEL
        // ==========================================
        public async Task<ApiResponse<List<AmenityResponseDto>>> GetCarouselAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get carousel request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Get Carousel
                var carousel = await _publicPageRepository
                    .GetCarsolesAsync(farmHouseId);

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = true,
                    Message = "Carousel fetched successfully.",
                    Data = carousel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting carousel.");

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = false,
                    Message = "Something went wrong."
                };
            }
        }


        // ==========================================
        // TOP 10 FEEDBACK
        // ==========================================
        public async Task<ApiResponse<List<FeedbackResponseDto>>> GetFeedbackAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get top feedback request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<List<FeedbackResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<List<FeedbackResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Get Feedback
                var feedbacks = await _publicPageRepository
                    .GetFeedbacksAsync(farmHouseId);

                return new ApiResponse<List<FeedbackResponseDto>>
                {
                    Success = true,
                    Message = "Top feedback fetched successfully.",
                    Data = feedbacks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting feedback.");

                return new ApiResponse<List<FeedbackResponseDto>>
                {
                    Success = false,
                    Message = "Something went wrong."
                };
            }
        }


        // ==========================================
        // GALLERY
        // ==========================================
        public async Task<ApiResponse<List<GalleryResponseDto>>> GetGalleryAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get gallery request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Get Gallery
                var galleries = await _publicPageRepository
                    .GetGalleryAync(farmHouseId);

                return new ApiResponse<List<GalleryResponseDto>>
                {
                    Success = true,
                    Message = "Gallery fetched successfully.",
                    Data = galleries
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting gallery.");

                return new ApiResponse<List<GalleryResponseDto>>
                {
                    Success = false,
                    Message = "Something went wrong."
                };
            }
        }

        public async Task<ApiResponse<AboutUsResponseDto>> GetAboutUsAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get AboutUs request received.");

                // 1. Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                       "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouseId.",
                        Data = null
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // 2. Get AboutUs Master and Active Features
                var aboutUs = await _aboutUsRepository
                    .GetAboutUsByFarmHouseIdAsync(farmHouseId);

                if (aboutUs == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs not found.",
                        Data = null
                    };
                }

                // 3. Map Master and Features into one DTO
                var response = new AboutUsResponseDto
                {
                    AboutUsId = aboutUs.AboutUsId,
                    HeroTitle = aboutUs.HeroTitle,
                    HeroSubtitle = aboutUs.HeroSubtitle,
                    HeroImageUrl = aboutUs.HeroImageUrl,
                    StoryTitle = aboutUs.StoryTitle,
                    StoryDescription = aboutUs.StoryDescription,
                    FarmHouseId = aboutUs.FarmHouseId,

                    Features = aboutUs.Features
                        .Where(f => !f.IsDelete)
                        .OrderBy(f => f.DisplayOrder)
                        .Select(f => new AboutUsFeatureResponseDto
                        {
                            FeatureId = f.FeatureId,
                            AboutUsId = f.AboutUsId,
                            Title = f.Title,
                            Description = f.Description ?? string.Empty,
                            Icon = f.Icon ?? string.Empty,
                            DisplayOrder = f.DisplayOrder
                        })
                        .ToList()
                };

                // 4. Return Response
                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message = "AboutUs retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving public AboutUs.");

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }


        public async Task<ApiResponse<FarmHouseResponseDto>> GetContactInfoAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get Contact Information request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse( farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<FarmHouseResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

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
                    Message = "Contact information retrieved successfully.",
                    Data = MapToResponseDto(farmHouse)
                };
                
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching FarmHouse.");

                return new ApiResponse<FarmHouseResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while fetching FarmHouse."
                };
            }
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
                OwnerUserId = farmHouse.OwnerUserId,
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
    }
}
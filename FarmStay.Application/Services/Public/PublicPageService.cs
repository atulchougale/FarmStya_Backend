
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Public;
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

        public PublicPageService(
            IPublicPageRepository publicPageRepository,
            ILogger<PublicPageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFarmHouseRepository farmHouseRepository)
        {
            _publicPageRepository = publicPageRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _farmHouseRepository = farmHouseRepository;
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
    }
}
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace FarmStay.Application.Services.Public
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ILogger<FeedbackService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;

        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            ILogger<FeedbackService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork, IFarmHouseRepository farmHouseRepository,
            IUserMembershipRepository userMembershipRepository)


        {
            _feedbackRepository = feedbackRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
        }


        public async Task<ApiResponse<FeedbackResponseDto>> SaveFeedbackAsync(FeedbackRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Feedback  Save request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Feedback Save request.");

                    return new ApiResponse<FeedbackResponseDto>
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

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<FeedbackResponseDto>
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
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse validated successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);


                // Step 4
                // Validate User belongs to FarmHouse

                var membership =
                    await _userMembershipRepository.GetMembershipAsync(
                        userId,
                        farmHouseId);

                if (membership == null)
                {
                    _logger.LogWarning(
                        "User is not a member of FarmHouse. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "You are not authorized to access this FarmHouse."
                    };
                }

                //Rating validation

                if (dto.Rating < 0.5m ||
                    dto.Rating > 5.0m ||
                    dto.Rating % 0.5m != 0)
                {
                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Rating must be between 0.5 and 5.0."
                    };
                }

                // Step 5
                // Create Feedback

                var feedback = new FeedBack
                {
                    FarmHouseId = farmHouseId,
                    Review = dto.Review,
                    Rating = dto.Rating,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,

                    ModifyBy = userId,
                    ModifyDate = DateTime.Now,
                    IsDelete = false
                };

                // Step 6
                // Save Feedback

                await _feedbackRepository.AddAsync(feedback);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Feedback created successfully. FarmHouseId: {FarmHouseId}, FeedbackId: {FeedbackId}",
                    farmHouseId,
                    feedback.FeedBackId);




                var response = new FeedbackResponseDto
                {
                    FeedbackId = feedback.FeedBackId,
                    FarmHouseId = farmHouseId,
                    Review = dto.Review,
                    Rating = dto.Rating,

                };

                // Step 8
                // Return Success Response

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = true,
                    Message = "Feedback saved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving Feedback.");

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong while saving feedback."
                };
            }
        }

        public async Task<ApiResponse<FeedbackResponseDto>> GetFeedbackByIdAsync(int feedbackId)
        {
            try
            {
                _logger.LogInformation(
                    "Get Feedback request received. FeedbackId: {FeedbackId}",
                    feedbackId);

                // Step 1
                // Get FarmHouseId from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 2
                // Validate FarmHouse

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 3
                // Get Feedback

                var feedback =
                    await _feedbackRepository.GetByIdAsync(feedbackId);

                if (feedback == null ||
                    feedback.FarmHouseId != farmHouseId)
                {
                    _logger.LogWarning(
                        "Feedback not found. FeedbackId: {FeedbackId}, FarmHouseId: {FarmHouseId}",
                        feedbackId,
                        farmHouseId);

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Feedback not found."
                    };
                }

                // Step 4
                // Create Response

                var response = new FeedbackResponseDto
                {
                    FeedbackId = feedback.FeedBackId,
                    FarmHouseId = feedback.FarmHouseId,
                    Review = feedback.Review,
                    Rating = feedback.Rating
                };

                // Step 5
                // Return Success

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = true,
                    Message = "Feedback retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while getting feedback. FeedbackId: {FeedbackId}",
                    feedbackId);

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred while getting the feedback."
                };
            }

        }
        public async Task<ApiResponse<List<FeedbackResponseDto>>> GetFeedbackAllAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Get all Feedback request received.");

                // Step 1
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

                // Step 2
                // Validate FarmHouse

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

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



                var feedback = await _feedbackRepository.GetAllAsync(farmHouseId);

                if (feedback == null || !feedback.Any())
                {
                    return new ApiResponse<List<FeedbackResponseDto>>
                    {
                        Success = false,
                        Message = "feedback found.",
                        Data = new List<FeedbackResponseDto>()
                    };

                }
                // Step 6: Convert Entity to DTO

                var feedbackDto = feedback
                    .Select(f => new FeedbackResponseDto
                    {
                        FeedbackId = f.FeedbackId,
                        Review = f.Review,
                        Rating = f.Rating,
                        FarmHouseId = f.FarmHouseId,
                        CreatedBy = f.CreatedBy,
                        CreatedByName = f.CreatedByName,
                        CreatedDate = f.CreatedDate
                    })
                    .ToList();

                // Step 7: Return Response

                return new ApiResponse<List<FeedbackResponseDto>>
                {
                    Success = true,
                    Message = "Feedback get successfully.",
                    Data = feedbackDto
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
                    Message = "An error occurred while getting feedback.",
                    Data = new List<FeedbackResponseDto>()
                };
            }
        }

        public async Task<ApiResponse<FeedbackResponseDto>> DeleteFeedbackAsync(int feedbackId)
        {
            try
            {
                _logger.LogInformation(
                    "Feedback delete request received. FeedbackId: {FeedbackId}",
                    feedbackId);

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Feedback delete request.");

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Unauthorized."
                    };
                }

                // Step 2: Get Feedback

                var feedback =
                    await _feedbackRepository.GetByIdAsync(feedbackId);

                if (feedback == null)
                {
                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "Feedback not found."
                    };
                }

                // Step 3: Check ownership

                if (feedback.CreatedBy != userId)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to delete FeedbackId: {FeedbackId}",
                        userId,
                        feedbackId);

                    return new ApiResponse<FeedbackResponseDto>
                    {
                        Success = false,
                        Message = "You are not authorized to delete this feedback."
                    };
                }

                // Step 4: Soft Delete

                await _feedbackRepository.DeleteAsync(
                    feedbackId,
                    userId);

                // Step 5: Prepare Response

                var response = new FeedbackResponseDto
                {
                    FeedbackId = feedback.FeedBackId,
                    FarmHouseId = feedback.FarmHouseId,
                    Review = feedback.Review,
                    Rating = feedback.Rating
                };

                // Step 6: Return Response

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = true,
                    Message = "Feedback deleted successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting feedback. FeedbackId: {FeedbackId}",
                    feedbackId);

                return new ApiResponse<FeedbackResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while deleting feedback."
                };
            }
        }
    }
}
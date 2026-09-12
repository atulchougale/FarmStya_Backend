using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
namespace FarmStay.Application.Services.Admin
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AmenityService> _logger;


        public AmenityService(
           IAmenityRepository amenityRepository,
           ILogger<AmenityService> logger,
           IHttpContextAccessor httpContextAccessor,
           IUnitOfWork unitOfWork,
           IFarmHouseRepository farmHouseRepository,
           IUserMembershipRepository userMembershipRepository)
        {
            _amenityRepository = amenityRepository;
            _logger = logger;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }



        public async Task<ApiResponse<AmenityResponseDto>> SaveAmenityAsync(AmenityRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Amenity Save request received.");

                // Step 1: Get UserId from JWT
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Amenity Save request.");

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Unauthorized."
                    };
                }

                // Step 2: Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(
                    farmHouseIdHeader,
                    out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 3: Validate FarmHouse
                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(
                        farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 4: Validate User Membership
                var membership =
                    await _userMembershipRepository.GetMembershipAsync(
                        userId,
                        farmHouseId);

                if (membership == null)
                {
                    _logger.LogWarning(
                        "User is not a member of FarmHouse. " +
                        "UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse."
                    };
                }

                // Step 5
                // Validate Image Name

                var title = dto.Title?.Trim();

                if (string.IsNullOrWhiteSpace(title))
                {
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Title is required."
                    };
                }

                // Step 6
                // Check Duplicate Image Name

                var existingImage =
                    await _amenityRepository.GetByTitleAsync(
                        farmHouseId,
                        title.ToLower());

                if (existingImage != null)
                {
                    _logger.LogWarning(
                        "Title already exists. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageName: {ImageName}",
                        userId,
                        farmHouseId,
                        title);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message =
                            "Title is already present in this FarmHouse. Please try a different name."
                    };
                }

                // Step 5: Create Amenity
                var amenity = new Amenity
                {
                    ImageUrl = dto.ImageUrl,
                    Title = title,
                    Description = dto.Description,
                    FarmHouseId = farmHouseId,                   
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsDelete = false,
                    IsAmenity = dto.IsAmenity,
                    IsCarasoul = dto.IsCarasoul
                };

                
                await _amenityRepository.AddAsync(amenity);

               
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Amenity created successfully. " +
                    "FarmHouseId: {FarmHouseId}, ImageId: {ImageId},UserId: {UserId}",
                    farmHouseId,
                    amenity.ImageId,
                    amenity.CreatedBy);

                // Step 8: Create Response DTO
                var response = new AmenityResponseDto
                {
                    ImageId = amenity.ImageId,
                    ImageUrl = amenity.ImageUrl,
                    Title = amenity.Title,
                    Description = amenity.Description,
                    FarmHouseId = amenity.FarmHouseId,
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    IsAmenity = dto.IsAmenity,
                    IsCarasoul = dto.IsCarasoul
                };

                // Step 9: Return Response
                return new ApiResponse<AmenityResponseDto>
                {
                    Success = true,
                    Message = "Amenity saved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving Amenity.");

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = false,
                    Message =
                        "Something went wrong while saving amenity."
                };
            }
        }

        public async Task<ApiResponse<AmenityResponseDto>> GetAmenityByIdAsync(int imageId)
        {
            try
            {


                _logger.LogInformation("Amenity  request received ById.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
               .User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
               .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Amenity request.");

                    return new ApiResponse<AmenityResponseDto>
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
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }



                // Step 3
                // Validate FarmHouse

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }


                // Step 4
                // Validate User belongs to FarmHouse
                var membership = await _userMembershipRepository.GetMembershipAsync(
                  userId,
                  farmHouseId);

                if (membership == null)
                {
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse."
                    };
                }

                // Step 5
                // Get Amenity by ImageId

                var Amenity = await _amenityRepository.GetById(imageId);


                if (Amenity == null)
                {
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity image not found."
                    };
                }


                if (Amenity.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity image does not belong to this FarmHouse."
                    };
                }


                var AmenityDto = new AmenityResponseDto
                {
                    ImageId = Amenity.ImageId,
                    ImageUrl = Amenity.ImageUrl,
                    Title = Amenity.Title,
                    Description = Amenity.Description,
                    IsCarasoul = Amenity.IsCarasoul,
                    IsAmenity = Amenity.IsAmenity,
                    FarmHouseId = Amenity.FarmHouseId,
                    UserId = Amenity.CreatedBy,
                    CreatedDate = Amenity.CreatedDate,
                };

                _logger.LogWarning(
                        "Amenity image get successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageName: {ImageName}",
                        userId,
                        farmHouseId,
                        Amenity.Title);


                return new ApiResponse<AmenityResponseDto>
                {
                    Success = true,
                    Message = "Amenity image get successfully.",
                    Data = AmenityDto
                };

                
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while getting Amenity image. ImageId: {ImageId}",
                    imageId);

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = false,
                    Message =
                        "An unexpected error occurred while getting the Amenity image."
                };
            }
        }

        public async Task<ApiResponse<List<AmenityResponseDto>>> GetAllAmenityAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Amenity get all request received.");

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Amenity request.");

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = new List<AmenityResponseDto>()
                    };
                }


                // Step 2: Get FarmHouseId from Header

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
                        Message = "Invalid FarmHouse.",
                        Data = new List<AmenityResponseDto>()
                    };
                }


                // Step 3: Validate FarmHouse

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = new List<AmenityResponseDto>()
                    };
                }


                // Step 4: Validate User belongs to FarmHouse

                var membership =
                    await _userMembershipRepository.GetMembershipAsync(
                        userId,
                        farmHouseId);

                if (membership == null)
                {
                    _logger.LogWarning(
                        "User is not authorized. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse.",
                        Data = new List<AmenityResponseDto>()
                    };
                }


                // Step 5: Get Amenity

                var Amenity =
                    await _amenityRepository.GetAllAsync(farmHouseId);

                if (Amenity == null || !Amenity.Any())
                {
                    return new ApiResponse<List<AmenityResponseDto>>
                    {
                        Success = false,
                        Message = "No Amenity images found.",
                        Data = new List<AmenityResponseDto>()
                    };
                }


                // Step 6: Convert Entity to DTO

                var AmenityDto = Amenity.Select(a => new AmenityResponseDto
                    {
                        ImageId = a.ImageId,
                        ImageUrl = a.ImageUrl,
                        Title = a.Title,
                        Description = a.Description,
                        IsCarasoul = a.IsCarasoul,
                        IsAmenity = a.IsAmenity,
                        FarmHouseId = a.FarmHouseId,
                        UserId = a.CreatedBy,
                        CreatedDate = a.CreatedDate,
                    })
                    .ToList();

                _logger.LogWarning(
                        "Amenity image get successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                // Step 7: Return Response

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = true,
                    Message = "Amenity images get successfully.",
                    Data = AmenityDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting Amenity images.");

                return new ApiResponse<List<AmenityResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while getting Amenity images.",
                    Data = new List<AmenityResponseDto>()
                };
            }
        }


        public async Task<ApiResponse<AmenityResponseDto>> UpdateAmenityAsync(AmenityRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Amenity update request received. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
                    dto.ImageId,
                    dto.FarmHouseId);

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Unauthorized Amenity request.");

                    return new ApiResponse<AmenityResponseDto>
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
                    return new ApiResponse<AmenityResponseDto>
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
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }


                // Step 4
                // Get Amenity by ImageId

                var Amenity =
                    await _amenityRepository.GetById(dto.ImageId);

                if (Amenity == null)
                {
                    _logger.LogWarning(
                        "Amenity image not found. ImageId: {ImageId}",
                        dto.ImageId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity image not found."
                    };
                }


                // Step 5
                // Validate Amenity belongs to FarmHouse

                if (Amenity.FarmHouseId != farmHouseId)
                {
                    _logger.LogWarning(
                        "Amenity does not belong to FarmHouse. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
                        dto.ImageId,
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity does not belong to this FarmHouse."
                    };
                }


                // Step 6
                // Update Amenity properties

                Amenity.ImageUrl = dto.ImageUrl;
                Amenity.Title = dto.Title;
                Amenity.Description = dto.Description;
                Amenity.IsAmenity = dto.IsAmenity;
                Amenity.IsCarasoul = dto.IsCarasoul;
                Amenity.ModifyBy = userId;
                Amenity.ModifyDate = DateTime.Now;


                // Step 7
                // Update Amenity in Database

                await _amenityRepository.UpdateAsync(Amenity);

                await _unitOfWork.SaveChangesAsync();

                // Step 8
                // Create Response DTO

                var AmenityResponse = new AmenityResponseDto
                {
                    ImageId = Amenity.ImageId,
                    ImageUrl = Amenity.ImageUrl,
                    Title = Amenity.Title,
                    Description = Amenity.Description,
                    IsCarasoul = Amenity.IsCarasoul,
                    IsAmenity = Amenity.IsAmenity,
                    FarmHouseId = Amenity.FarmHouseId,
                    UserId = Amenity.CreatedBy,
                };

                _logger.LogWarning(
                        "Amenity image updated successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageName: {ImageName}",
                        userId,
                        farmHouseId,
                        Amenity.Title);
                // Step 9
                // Return Success Response

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = true,
                    Message = "Amenity image updated successfully.",
                    Data = AmenityResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating Amenity for FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    dto.FarmHouseId,
                    dto.ImageId);

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating Amenity image."
                };
            }
        }

        public async Task<ApiResponse<AmenityResponseDto>> DeleteAmenityAsync(int imageId)
        {
            try
            {
                _logger.LogInformation(
                    "Amenity Image delete request received. ImageId: {ImageId}",
                    imageId);

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Amenity Image delete request.");

                    return new ApiResponse<AmenityResponseDto>
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
                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }


                // Step 3
                // Validate FarmHouse

                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 4
                // Get Amenity by ImageId

                var Amenity =
                    await _amenityRepository.GetById(imageId);

                if (Amenity == null)
                {
                    _logger.LogWarning(
                        "Amenity image not found for delete. ImageId: {ImageId}",
                        imageId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity image not found for delete."
                    };
                }


                // Step 5
                // Validate Amenity belongs to FarmHouse

                if (Amenity.FarmHouseId != farmHouseId)
                {
                    _logger.LogWarning(
                        "Amenity does not belong to FarmHouse for delete. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
                        imageId,
                        farmHouseId);

                    return new ApiResponse<AmenityResponseDto>
                    {
                        Success = false,
                        Message = "Amenity image does not belong to this FarmHouse."
                    };
                }


                // Step 6
                // Update Amenity properties

                Amenity.IsDelete = true;
                Amenity.ModifyBy = userId;
                Amenity.ModifyDate = DateTime.Now;


                // Step 7
                // Update Amenity in Database

                await _amenityRepository.UpdateAsync(Amenity);

                await _unitOfWork.SaveChangesAsync();

                // Step 5: Prepare Response

                var response = new AmenityResponseDto
                {
                    ImageId = Amenity.ImageId,
                    Title = Amenity.Title,
                    IsCarasoul = Amenity.IsCarasoul,
                    IsAmenity = Amenity.IsAmenity,
                };

                _logger.LogWarning(
                       "Amenity deleted successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageName: {ImageName}",
                       userId,
                       farmHouseId,
                       Amenity.Title);

                // Step 6: Return Response

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = true,
                    Message = "Amenity deleted successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting Amenity. ImageId: {ImageId}",
                    imageId);

                return new ApiResponse<AmenityResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while deleting image in Amenity."
                };
            }
        }
    }
}
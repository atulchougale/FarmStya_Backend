using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace FarmStay.Application.Services.Admin
{
    public class GalleryService : IGalleryService
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GalleryService> _logger;

        public GalleryService(
            IGalleryRepository galleryRepository,
            ILogger<GalleryService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IFarmHouseRepository farmHouseRepository,
            IUserMembershipRepository userMembershipRepository)
        {
            _galleryRepository = galleryRepository;
            _logger = logger;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<GalleryResponseDto>> SaveGalleryAsync(GalleryRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Gallery Save request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Gallery Save request.");

                    return new ApiResponse<GalleryResponseDto>
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

                    return new ApiResponse<GalleryResponseDto>
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

                    return new ApiResponse<GalleryResponseDto>
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

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "You are not authorized to access this FarmHouse."
                    };
                }

                // Step 5
                // Validate Image Name

                var imageName = dto.ImageName?.Trim();

                if (string.IsNullOrWhiteSpace(imageName))
                {
                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Image name is required."
                    };
                }

                // Step 6
                // Check Duplicate Image Name

                var existingImage =
                    await _galleryRepository.GetByImageNameAsync(
                        farmHouseId,
                        imageName.ToLower());

                if (existingImage != null)
                {
                    _logger.LogWarning(
                        "Image name already exists. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageName: {ImageName}",
                        userId,
                        farmHouseId,
                        imageName);

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message =
                            "Image Name is already present in this FarmHouse. Please try a different name."
                    };
                }

                // Step 7
                // Create Gallery Image

                var image = new Gallery
                {
                    ImageName = imageName,
                    ImageUrl = dto.ImageUrl,
                    Category = dto.Category,
                    Description = dto.Description,
                    DisplayOrder = dto.DisplayOrder,
                    IsFavorite = dto.IsFavorite,

                    FarmHouseId = farmHouseId,

                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,

                    IsDelete = false
                };

                await _galleryRepository.AddAsync(image);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Gallery image created successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    userId,
                    farmHouseId,
                    image.ImageId);

                // Step 8
                // Return Response

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = true,
                    Message = "Gallery saved successfully.",
                    Data = new GalleryResponseDto
                    {
                        ImageId = image.ImageId,
                        ImageName = image.ImageName,
                        ImageUrl = image.ImageUrl,
                        Category = image.Category,
                        Description = image.Description,
                        DisplayOrder = image.DisplayOrder,
                        IsFavorite = image.IsFavorite,
                        FarmHouseId = farmHouseId,
                        CreatedBy = userId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while saving Gallery. ImageName: {ImageName}",
                    dto.ImageName);

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = false,
                    Message =
                        "An unexpected error occurred while saving the Gallery."
                };
            }
        }


        public async Task<ApiResponse<GalleryResponseDto>> GetGalleryByIdAsync(int imageId)
        {
            try
            {


                _logger.LogInformation("Gallery  request received.");

                // Step 1
                // Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
               .User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
               .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Gallery request.");

                    return new ApiResponse<GalleryResponseDto>
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
                    return new ApiResponse<GalleryResponseDto>
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
                    return new ApiResponse<GalleryResponseDto>
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
                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse."
                    };
                }

                // Step 5
                // Get Gallery by ImageId

                var gallery = await _galleryRepository.GetByIdAsync(imageId);


                if (gallery == null)
                {
                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery image not found."
                    };
                }


                if (gallery.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery image does not belong to this FarmHouse."
                    };
                }


                var galleryDto = new GalleryResponseDto
                {
                    ImageId = gallery.ImageId,
                    ImageUrl = gallery.ImageUrl,
                    ImageName = gallery.ImageName,
                    Category = gallery.Category,
                    Description = gallery.Description,
                    DisplayOrder = gallery.DisplayOrder,
                    FarmHouseId = gallery.FarmHouseId,
                    CreatedBy = gallery.CreatedBy,
                };

                _logger.LogInformation(
                    "Gallery image Get successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    userId,
                    farmHouseId,
                    gallery.ImageId);

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = true,
                    Message = "Gallery image get successfully.",
                    Data = galleryDto
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while getting Gallery image. ImageId: {ImageId}",
                    imageId);

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = false,
                    Message =
                        "An unexpected error occurred while getting the Gallery image."
                };
            }
        }


        public async Task<ApiResponse<List<GalleryResponseDto>>> GetAllGalleryAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Gallery get all request received.");

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Gallery request.");

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = new List<GalleryResponseDto>()
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

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = new List<GalleryResponseDto>()
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

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = new List<GalleryResponseDto>()
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

                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse.",
                        Data = new List<GalleryResponseDto>()
                    };
                }


                // Step 5: Get Gallery

                var gallery =
                    await _galleryRepository.GetAllAsync(farmHouseId);

                if (gallery == null || !gallery.Any())
                {
                    return new ApiResponse<List<GalleryResponseDto>>
                    {
                        Success = false,
                        Message = "No gallery images found.",
                        Data = new List<GalleryResponseDto>()
                    };
                }


                // Step 6: Convert Entity to DTO

                var galleryDto = gallery
                    .Select(g => new GalleryResponseDto
                    {
                        ImageId = g.ImageId,
                        ImageUrl = g.ImageUrl,
                        ImageName = g.ImageName,
                        Category = g.Category,
                        Description = g.Description,
                        DisplayOrder = g.DisplayOrder,
                        FarmHouseId = g.FarmHouseId
                    })
                    .ToList();


                // Step 7: Return Response

                return new ApiResponse<List<GalleryResponseDto>>
                {
                    Success = true,
                    Message = "Gallery images get successfully.",
                    Data = galleryDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting gallery images.");

                return new ApiResponse<List<GalleryResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while getting gallery images.",
                    Data = new List<GalleryResponseDto>()
                };
            }
        }


        public async Task<ApiResponse<GalleryResponseDto>> UpdateGalleryAsync(GalleryRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Gallery update request received. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
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
                    _logger.LogWarning("Unauthorized Gallery request.");

                    return new ApiResponse<GalleryResponseDto>
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
                    return new ApiResponse<GalleryResponseDto>
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

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }


                // Step 4
                // Get Gallery by ImageId

                var gallery =
                    await _galleryRepository.GetByIdAsync(dto.ImageId);

                if (gallery == null)
                {
                    _logger.LogWarning(
                        "Gallery image not found. ImageId: {ImageId}",
                        dto.ImageId);

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery image not found."
                    };
                }


                // Step 5
                // Validate Gallery belongs to FarmHouse

                if (gallery.FarmHouseId != farmHouseId)
                {
                    _logger.LogWarning(
                        "Gallery does not belong to FarmHouse. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
                        dto.ImageId,
                        farmHouseId);

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery does not belong to this FarmHouse."
                    };
                }


                // Step 6
                // Update Gallery properties

                gallery.ImageUrl = dto.ImageUrl;
                gallery.ImageName = dto.ImageName;
                gallery.Category = dto.Category;
                gallery.Description = dto.Description;
                gallery.DisplayOrder = dto.DisplayOrder;
                gallery.IsFavorite = dto.IsFavorite;
                gallery.ModifyBy = userId;
                gallery.ModifyDate = DateTime.Now;


                // Step 7
                // Update Gallery in Database

                await _galleryRepository.UpdateAsync(gallery);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Gallery image updated successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    userId,
                    farmHouseId,
                    gallery.ImageId);

                // Step 8
                // Create Response DTO

                var galleryResponse = new GalleryResponseDto
                {
                    ImageId = gallery.ImageId,
                    ImageUrl = gallery.ImageUrl,
                    ImageName = gallery.ImageName,
                    Category = gallery.Category,
                    Description = gallery.Description,
                    DisplayOrder = gallery.DisplayOrder,
                    IsFavorite = gallery.IsFavorite,
                    FarmHouseId = gallery.FarmHouseId
                };


                // Step 9
                // Return Success Response

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = true,
                    Message = "Gallery image updated successfully.",
                    Data = galleryResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating gallery for FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    dto.FarmHouseId,
                    dto.ImageId);

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating gallery image."
                };
            }
        }

        public async Task<ApiResponse<GalleryResponseDto>> DeleteGalleryAsync(int imageId)
        {
            try
            {
                _logger.LogInformation(
                    "Gallery Image delete request received. ImageId: {ImageId}",
                    imageId);

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized Gallery Image delete request.");

                    return new ApiResponse<GalleryResponseDto>
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
                    return new ApiResponse<GalleryResponseDto>
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

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 4
                // Get Gallery by ImageId

                var gallery =
                    await _galleryRepository.GetByIdAsync(imageId);

                if (gallery == null)
                {
                    _logger.LogWarning(
                        "Gallery image not found for delete. ImageId: {ImageId}",
                        imageId);

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery image not found for delete."
                    };
                }


                // Step 5
                // Validate Gallery belongs to FarmHouse

                if (gallery.FarmHouseId != farmHouseId)
                {
                    _logger.LogWarning(
                        "Gallery does not belong to FarmHouse for delete. ImageId: {ImageId}, FarmHouseId: {FarmHouseId}",
                        imageId,
                        farmHouseId);

                    return new ApiResponse<GalleryResponseDto>
                    {
                        Success = false,
                        Message = "Gallery image does not belong to this FarmHouse."
                    };
                }


                // Step 6
                // Update Gallery properties

                gallery.IsDelete = true;
                gallery.ModifyBy = userId;
                gallery.ModifyDate = DateTime.Now;


                // Step 7
                // Update Gallery in Database

                await _galleryRepository.UpdateAsync(gallery);

                await _unitOfWork.SaveChangesAsync();

                // Step 5: Prepare Response

                var response = new GalleryResponseDto
                {
                    ImageId = gallery.ImageId,
                    ImageName = gallery.ImageName
                };

                _logger.LogInformation(
                    "Gallery image Deleted successfully. UserId: {UserId}, FarmHouseId: {FarmHouseId}, ImageId: {ImageId}",
                    userId,
                    farmHouseId,
                    gallery.ImageId);
                // Step 6: Return Response

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = true,
                    Message = "Gallery deleted successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting Gallery. ImageId: {ImageId}",
                    imageId);

                return new ApiResponse<GalleryResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while deleting image in gallery."
                };
            }
        }

        public async Task<ApiResponse<List<string>>> GetCategoryListAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Gallery category request received.");

                // Step 1: Get UserId from JWT

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = new List<string>()
                    };
                }

                // Step 2: Get FarmHouseId from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = new List<string>()
                    };
                }

                // Step 3: Validate FarmHouse

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = new List<string>()
                    };
                }

                // Step 4: Validate User belongs to FarmHouse

                var membership =
                    await _userMembershipRepository.GetMembershipAsync(
                        userId,
                        farmHouseId);

                if (membership == null)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse.",
                        Data = new List<string>()
                    };
                }

                // Step 5: Get Categories

                var categories =
                    await _galleryRepository.GetCategoryListAsync(
                        farmHouseId);

                if (categories == null || !categories.Any())
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = "Categories not found.",
                        Data = new List<string>()
                    };
                }

                // Step 6: Return Response

                return new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = "Categories received successfully.",
                    Data = categories
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting category list.");

                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error while getting category list.",
                    Data = new List<string>()
                };
            }
        }
    }
}
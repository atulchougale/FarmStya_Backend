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
    public class AboutUsService : IAboutUsService
    {
        private readonly IAboutUsRepository _aboutUsRepository;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AboutUsService> _logger;

        public AboutUsService(
            IAboutUsRepository aboutUsRepository,
            ILogger<AboutUsService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IFarmHouseRepository farmHouseRepository,
            IUserMembershipRepository userMembershipRepository)
        {
            _aboutUsRepository = aboutUsRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
        }



        public async Task<ApiResponse<AboutUsResponseDto>> SaveAboutUsAsync( AboutUsRequestDto dto)
        {
            try
            {
               
                // 1. GET FARMHOUSE ID FROM HEADER
               

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = null
                    };
                }

              
                // 2. GET USER ID FROM JWT
               

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning(
                        "Unauthorized AboutUs save request.");

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = null
                    };
                }

               
                // 3. VALIDATE FARMHOUSE
              

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = null
                    };
                }

                // 4. VALIDATE USER MEMBERSHIP
               
                var membership =
                    await _userMembershipRepository
                        .GetMembershipAsync(userId, farmHouseId);

                if (membership == null)
                {
                    _logger.LogWarning(
                        "User is not a member of FarmHouse. " +
                        "UserId: {UserId}, FarmHouseId: {FarmHouseId}",
                        userId,
                        farmHouseId);

                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse.",
                        Data = null
                    };
                }

               
                // 5. GET EXISTING ACTIVE ABOUT US
                

                var aboutUs =
                    await _aboutUsRepository
                        .GetAboutUsByFarmHouseIdAsync(farmHouseId);

                // 6. CREATE OR UPDATE ABOUT US
               
                if (aboutUs == null)
                {
                   
                    // CREATE NEW ABOUT US
                    

                    aboutUs = new AboutUs
                    {
                        HeroTitle = dto.HeroTitle,
                        HeroSubtitle = dto.HeroSubtitle,
                        HeroImageUrl = dto.HeroImageUrl,

                        StoryTitle = dto.StoryTitle,
                        StoryDescription = dto.StoryDescription,

                        FarmHouseId = farmHouseId,

                        IsDelete = false,

                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow,

                        ModifyBy = userId,
                        ModifyDate = DateTime.UtcNow
                    };

                    await _aboutUsRepository
                        .AddAboutUsAsync(aboutUs);

                    // Save here because we need AboutUsId
                    // before adding features.
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "New AboutUs created. AboutUsId: {AboutUsId}, FarmHouseId: {FarmHouseId}",
                        aboutUs.AboutUsId,
                        farmHouseId);
                }
                else
                {
                    // UPDATE EXISTING ABOUT US
                    
                    aboutUs.HeroTitle = dto.HeroTitle;
                    aboutUs.HeroSubtitle = dto.HeroSubtitle;
                    aboutUs.HeroImageUrl = dto.HeroImageUrl;

                    aboutUs.StoryTitle = dto.StoryTitle;
                    aboutUs.StoryDescription = dto.StoryDescription;

                    aboutUs.IsDelete = false;

                    aboutUs.ModifyBy = userId;
                    aboutUs.ModifyDate = DateTime.UtcNow;

                    await _aboutUsRepository
                        .UpdateAboutUsAsync(aboutUs);

                    _logger.LogInformation(
                        "AboutUs updated. AboutUsId: {AboutUsId}, FarmHouseId: {FarmHouseId}",
                        aboutUs.AboutUsId,
                        farmHouseId);
                }

               
                // 7. ADD / UPDATE FEATURES
                

                if (dto.Features != null &&
                    dto.Features.Any())
                {
                    foreach (var featureDto in dto.Features)
                    {
                        
                        // . NEW FEATURE
                       

                        if (featureDto.FeatureId == 0)
                        {
                            var newFeature = new AboutUsFeature
                            {
                                AboutUsId = aboutUs.AboutUsId,

                                Title = featureDto.Title,
                                Description = featureDto.Description,
                                Icon = featureDto.Icon,
                                DisplayOrder = featureDto.DisplayOrder,

                                IsDelete = false
                            };

                            await _aboutUsRepository
                                .AddFeatureAsync(newFeature);

                            _logger.LogInformation(
                                "New AboutUs feature added. " +
                                "AboutUsId: {AboutUsId}, Title: {Title}",
                                aboutUs.AboutUsId,
                                featureDto.Title);
                        }
                        else
                        {
                           
                            // GET EXISTING FEATURE
                           

                            var existingFeature =
                                await _aboutUsRepository
                                    .GetFeatureByIdAsync(
                                        featureDto.FeatureId);

                            if (existingFeature == null)
                            {
                                return new ApiResponse<AboutUsResponseDto>
                                {
                                    Success = false,
                                    Message =
                                        $"Feature with ID " +
                                        $"{featureDto.FeatureId} not found.",
                                    Data = null
                                };
                            }

                           
                            //  SECURITY CHECK
                           
                            if (existingFeature.AboutUsId !=
                                aboutUs.AboutUsId)
                            {
                                _logger.LogWarning(
                                    "Feature does not belong to AboutUs. " +
                                    "FeatureId: {FeatureId}, AboutUsId: {AboutUsId}",
                                    featureDto.FeatureId,
                                    aboutUs.AboutUsId);

                                return new ApiResponse<AboutUsResponseDto>
                                {
                                    Success = false,
                                    Message =
                                        "Feature does not belong to this AboutUs.",
                                    Data = null
                                };
                            }

                          
                            //  UPDATE FEATURE
                            

                            existingFeature.Title =
                                featureDto.Title;

                            existingFeature.Description =
                                featureDto.Description;

                            existingFeature.Icon =
                                featureDto.Icon;

                            existingFeature.DisplayOrder =
                                featureDto.DisplayOrder;

                            existingFeature.IsDelete = false;

                            await _aboutUsRepository
                                .UpdateFeatureAsync(existingFeature);

                            _logger.LogInformation(
                                "AboutUs feature updated. FeatureId: {FeatureId}",
                                existingFeature.FeatureId);
                        }
                    }
                }

               
                // 8. SAVE ALL CHANGES
               
                await _unitOfWork.SaveChangesAsync();

               
                // 9. GET UPDATED ABOUT US WITH FEATURES
               

                var savedAboutUs =
                    await _aboutUsRepository
                        .GetAboutUsAsync(aboutUs.AboutUsId);

                if (savedAboutUs == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Unable to retrieve saved AboutUs.",
                        Data = null
                    };
                }

               
                // 10. ENTITY TO RESPONSE DTO
              

                var response = new AboutUsResponseDto
                {
                    AboutUsId = savedAboutUs.AboutUsId,

                    HeroTitle = savedAboutUs.HeroTitle,
                    HeroSubtitle = savedAboutUs.HeroSubtitle,
                    HeroImageUrl = savedAboutUs.HeroImageUrl,

                    StoryTitle = savedAboutUs.StoryTitle,
                    StoryDescription = savedAboutUs.StoryDescription,

                    FarmHouseId = savedAboutUs.FarmHouseId,

                    Features = savedAboutUs.Features
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new AboutUsFeatureResponseDto
                        {
                            FeatureId = x.FeatureId,
                            Title = x.Title,
                            Description = x.Description,
                            Icon = x.Icon,
                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList()
                };

                
                // 11. SUCCESS RESPONSE
              
                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message = "AboutUs saved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving AboutUs. FarmHouseId: {FarmHouseId}",
                    _httpContextAccessor.HttpContext?
                        .Request
                        .Headers["FarmHouseId"]
                        .FirstOrDefault());

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message,
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<AboutUsResponseDto>> GetAboutUsAsync()
        {
            try
            {
                //  FarmHouseId from header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouseId.",
                        Data = null
                    };
                }

                // Get AboutUs with Features
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

                //  response DTO
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
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new AboutUsFeatureResponseDto
                        {
                            FeatureId = x.FeatureId,
                            AboutUsId = x.AboutUsId,
                            Title = x.Title,
                            Description = x.Description ?? string.Empty,
                            Icon = x.Icon ?? string.Empty,
                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList()
                };

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message = "AboutUs retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting AboutUs.");

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }



        public async Task<ApiResponse<AboutUsResponseDto>> UpdateAboutUsAsync(AboutUsRequestDto dto)
        {
            try
            {
                // =========================================================
                // 1. GET FARMHOUSE ID FROM HEADER
                // =========================================================

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 2. GET USER ID FROM JWT
                // =========================================================

                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = null
                    };
                }

                // =========================================================
                // 3. VALIDATE FARMHOUSE
                // =========================================================

                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 4. VALIDATE USER MEMBERSHIP
                // =========================================================

                var membership = await _userMembershipRepository
                    .GetMembershipAsync(userId, farmHouseId);

                if (membership == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "You are not authorized to access this FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 5. GET ABOUT US
                // =========================================================

                var aboutUs = await _aboutUsRepository.GetAboutUsAsync(dto.AboutUsId);

                if (aboutUs == null || aboutUs.IsDelete)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs not found.",
                        Data = null
                    };
                }

                // =========================================================
                // 6. VALIDATE FARMHOUSE OWNERSHIP
                // =========================================================

                if (aboutUs.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs does not belong to this FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 7. GET EXISTING ACTIVE FEATURES
                // =========================================================

                List<AboutUsFeature> existingFeatures = new();

                if (dto.Features != null)
                {
                    existingFeatures = await _aboutUsRepository.GetFeaturesAsync(dto.AboutUsId);

                    // -----------------------------------------------------
                    // VALIDATE EXISTING FEATURE IDS BEFORE UPDATING
                    // -----------------------------------------------------

                    foreach (var featureDto in dto.Features)
                    {
                        // FeatureId = 0 means a new feature.
                        if (featureDto.FeatureId == 0)
                        {
                            continue;
                        }

                        var existingFeature = await _aboutUsRepository
                            .GetFeatureByIdAsync(featureDto.FeatureId);

                        // Deleted features are also treated as not found.
                        // They must NOT be reactivated automatically.
                        if (existingFeature == null)
                        {
                            return new ApiResponse<AboutUsResponseDto>
                            {
                                Success = false,
                                Message = $"Feature with ID {featureDto.FeatureId} not found.",
                                Data = null
                            };
                        }

                        // Validate that the feature belongs to this AboutUs.
                        if (existingFeature.AboutUsId != aboutUs.AboutUsId)
                        {
                            return new ApiResponse<AboutUsResponseDto>
                            {
                                Success = false,
                                Message = "Feature does not belong to this AboutUs.",
                                Data = null
                            };
                        }
                    }
                }

                // =========================================================
                // 8. UPDATE ABOUT US MASTER
                // =========================================================

                aboutUs.HeroTitle = dto.HeroTitle;
                aboutUs.HeroSubtitle = dto.HeroSubtitle;
                aboutUs.HeroImageUrl = dto.HeroImageUrl;

                aboutUs.StoryTitle = dto.StoryTitle;
                aboutUs.StoryDescription = dto.StoryDescription;

                aboutUs.ModifyBy = userId;
                aboutUs.ModifyDate = DateTime.UtcNow;

                await _aboutUsRepository.UpdateAboutUsAsync(aboutUs);

                // =========================================================
                // 9. ADD / UPDATE FEATURES
                // =========================================================

                if (dto.Features != null)
                {
                    foreach (var featureDto in dto.Features)
                    {
                        // -------------------------------------------------
                        // CASE A: NEW FEATURE
                        // -------------------------------------------------

                        if (featureDto.FeatureId == 0)
                        {
                            var newFeature = new AboutUsFeature
                            {
                                AboutUsId = aboutUs.AboutUsId,

                                Title = featureDto.Title,
                                Description = featureDto.Description,
                                Icon = featureDto.Icon,
                                DisplayOrder = featureDto.DisplayOrder,

                                IsDelete = false
                            };

                            await _aboutUsRepository
                                .AddFeatureAsync(newFeature);

                            continue;
                        }

                        // -------------------------------------------------
                        // CASE B: UPDATE EXISTING ACTIVE FEATURE
                        // -------------------------------------------------

                        var existingFeature = await _aboutUsRepository
                            .GetFeatureByIdAsync(featureDto.FeatureId);

                        // Safety check: do not reactivate deleted features.
                        if (existingFeature == null)
                        {
                            return new ApiResponse<AboutUsResponseDto>
                            {
                                Success = false,
                                Message = $"Feature with ID {featureDto.FeatureId} not found.",
                                Data = null
                            };
                        }

                        if (existingFeature.AboutUsId != aboutUs.AboutUsId)
                        {
                            return new ApiResponse<AboutUsResponseDto>
                            {
                                Success = false,
                                Message = "Feature does not belong to this AboutUs.",
                                Data = null
                            };
                        }

                        existingFeature.Title = featureDto.Title;
                        existingFeature.Description = featureDto.Description;
                        existingFeature.Icon = featureDto.Icon;
                        existingFeature.DisplayOrder = featureDto.DisplayOrder;

                        // IMPORTANT:
                        // Do not set IsDelete = false here.
                        // A soft-deleted feature must never be reactivated
                        // through this update method.

                        await _aboutUsRepository.UpdateFeatureAsync(existingFeature);
                    }

                    // =====================================================
                    // 10. SOFT DELETE REMOVED FEATURES
                    // =====================================================

                    var requestedFeatureIds = dto.Features
                        .Where(x => x.FeatureId > 0)
                        .Select(x => x.FeatureId)
                        .ToHashSet();

                    foreach (var existingFeature in existingFeatures)
                    {
                        // If an existing active feature is missing from the
                        // request, soft-delete it instead of permanently
                        // deleting it from the database.

                        if (!requestedFeatureIds.Contains(existingFeature.FeatureId))
                        {
                            await _aboutUsRepository.DeleteFeatureAsync(existingFeature.FeatureId);
                        }
                    }
                }

                // =========================================================
                // 11. SAVE CHANGES
                // =========================================================

                await _unitOfWork.SaveChangesAsync();

                // =========================================================
                // 12. GET UPDATED ABOUT US WITH ACTIVE FEATURES
                // =========================================================

                var updatedAboutUs = await _aboutUsRepository.GetAboutUsAsync(dto.AboutUsId);

                if (updatedAboutUs == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs could not be retrieved.",
                        Data = null
                    };
                }

                // =========================================================
                // 13. PREPARE RESPONSE
                // =========================================================

                var response = new AboutUsResponseDto
                {
                    AboutUsId = updatedAboutUs.AboutUsId,

                    HeroTitle = updatedAboutUs.HeroTitle,
                    HeroSubtitle = updatedAboutUs.HeroSubtitle,
                    HeroImageUrl = updatedAboutUs.HeroImageUrl,

                    StoryTitle = updatedAboutUs.StoryTitle,
                    StoryDescription = updatedAboutUs.StoryDescription,

                    FarmHouseId = updatedAboutUs.FarmHouseId,

                    Features = updatedAboutUs.Features
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new AboutUsFeatureResponseDto
                        {
                            FeatureId = x.FeatureId,
                            AboutUsId = x.AboutUsId,

                            Title = x.Title,
                            Description = x.Description ?? string.Empty,
                            Icon = x.Icon ?? string.Empty,
                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList()
                };

                // =========================================================
                // 14. SUCCESS RESPONSE
                // =========================================================

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message = "AboutUs and Features updated successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating AboutUs and Features.");

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<AboutUsResponseDto>> DeleteAboutUsAsync(int aboutUsId)
        {
            try
            {
                // 1. Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouseId.",
                        Data = null
                    };
                }

                // 2. Get UserId from JWT
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Unauthorized.",
                        Data = null
                    };
                }

                // 3. Check FarmHouse exists
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouseId.",
                        Data = null
                    };
                }

                // 4. Check User Membership
                var membership = await _userMembershipRepository
                    .GetMembershipAsync(userId, farmHouseId);

                if (membership == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "You are not authorized to access this FarmHouse.",
                        Data = null
                    };
                }

                // 5. Get Existing AboutUs with Active Features
                var aboutUs = await _aboutUsRepository
                    .GetAboutUsAsync(aboutUsId);

                if (aboutUs == null || aboutUs.IsDelete)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs not found.",
                        Data = null
                    };
                }

                // 6. Check AboutUs belongs to the requested FarmHouse
                if (aboutUs.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs does not belong to this FarmHouse.",
                        Data = null
                    };
                }

                // 7. Soft Delete All Active Features First
                foreach (var feature in aboutUs.Features.Where(x => !x.IsDelete))
                {
                    feature.IsDelete = true;

                    await _aboutUsRepository
                        .UpdateFeatureAsync(feature);
                }

                // 8. Soft Delete AboutUs Master After Features
                aboutUs.IsDelete = true;
                aboutUs.ModifyBy = userId;
                aboutUs.ModifyDate = DateTime.UtcNow;

                await _aboutUsRepository
                    .UpdateAboutUsAsync(aboutUs);

                // 9. Save All Changes Together
                await _unitOfWork.SaveChangesAsync();

                // 10. Create Response DTO
                // Features will be empty because all active features
                // have been soft-deleted.
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
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new AboutUsFeatureResponseDto
                        {
                            FeatureId = x.FeatureId,
                            AboutUsId = x.AboutUsId,
                            Title = x.Title,
                            Description = x.Description ?? string.Empty,
                            Icon = x.Icon ?? string.Empty,
                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList()
                };

                // 11. Return Success Response
                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message = "AboutUs and its Features deleted successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting AboutUs with ID {AboutUsId}",
                    aboutUsId);

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong while deleting AboutUs.",
                    Data = null
                };
            }
        }
    }
}
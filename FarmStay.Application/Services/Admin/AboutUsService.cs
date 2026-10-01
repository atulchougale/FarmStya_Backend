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



        public async Task<ApiResponse<AboutUsResponseDto>> SaveAboutUsAsync(
     AboutUsRequestDto dto)
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

        
        
        public async Task<ApiResponse<AboutUsResponseDto>> UpdateAboutUsAsync (int aboutUsId,AboutUsRequestDto dto)
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

                if (!int.TryParse(
                        farmHouseIdHeader,
                        out int farmHouseId))
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

                if (!int.TryParse(
                        userIdClaim,
                        out int userId))
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

                var farmHouse =
                    await _farmHouseRepository
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
                // 4. VALIDATE MEMBERSHIP
                // =========================================================

                var membership =
                    await _userMembershipRepository
                        .GetMembershipAsync(
                            userId,
                            farmHouseId);

                if (membership == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message =
                            "You are not authorized to access this FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 5. GET ABOUT US
                // =========================================================

                var aboutUs =
                    await _aboutUsRepository
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

                // =========================================================
                // 6. SECURITY CHECK
                // =========================================================

                if (aboutUs.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message =
                            "AboutUs does not belong to this FarmHouse.",
                        Data = null
                    };
                }

                // =========================================================
                // 7. UPDATE ABOUT US
                // =========================================================

                aboutUs.HeroTitle = dto.HeroTitle;
                aboutUs.HeroSubtitle = dto.HeroSubtitle;
                aboutUs.HeroImageUrl = dto.HeroImageUrl;

                aboutUs.StoryTitle = dto.StoryTitle;
                aboutUs.StoryDescription = dto.StoryDescription;

                aboutUs.ModifyBy = userId;
                aboutUs.ModifyDate = DateTime.UtcNow;

                await _aboutUsRepository
                    .UpdateAboutUsAsync(aboutUs);

                // =========================================================
                // 8. ADD / UPDATE FEATURES
                // =========================================================

                if (dto.Features != null)
                {
                    foreach (var featureDto in dto.Features)
                    {
                        // -------------------------------------------------
                        // NEW FEATURE
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
                        // EXISTING FEATURE
                        // -------------------------------------------------

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

                        // Security check
                        if (existingFeature.AboutUsId !=
                            aboutUs.AboutUsId)
                        {
                            return new ApiResponse<AboutUsResponseDto>
                            {
                                Success = false,
                                Message =
                                    "Feature does not belong to this AboutUs.",
                                Data = null
                            };
                        }

                        existingFeature.Title =
                            featureDto.Title;

                        existingFeature.Description =
                            featureDto.Description;

                        existingFeature.Icon =
                            featureDto.Icon;

                        existingFeature.DisplayOrder =
                            featureDto.DisplayOrder;

                        // If a previously soft-deleted feature
                        // is updated, make it active again.
                        existingFeature.IsDelete = false;

                        await _aboutUsRepository
                            .UpdateFeatureAsync(existingFeature);
                    }
                }

                // =========================================================
                // 9. SAVE
                // =========================================================

                await _unitOfWork.SaveChangesAsync();

                // =========================================================
                // 10. GET UPDATED DATA
                // =========================================================

                var updatedAboutUs =
                    await _aboutUsRepository
                        .GetAboutUsByFarmHouseIdAsync(farmHouseId);

                if (updatedAboutUs == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message =
                            "AboutUs could not be retrieved.",
                        Data = null
                    };
                }

                // =========================================================
                // 11. RESPONSE
                // =========================================================

                var response = new AboutUsResponseDto
                {
                    AboutUsId = updatedAboutUs.AboutUsId,

                    HeroTitle = updatedAboutUs.HeroTitle,
                    HeroSubtitle = updatedAboutUs.HeroSubtitle,
                    HeroImageUrl = updatedAboutUs.HeroImageUrl,

                    StoryTitle = updatedAboutUs.StoryTitle,
                    StoryDescription =
                        updatedAboutUs.StoryDescription,

                    FarmHouseId =
                        updatedAboutUs.FarmHouseId,

                    Features = updatedAboutUs.Features
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new AboutUsFeatureResponseDto
                        {
                            FeatureId = x.FeatureId,

                            AboutUsId = x.AboutUsId,

                            Title = x.Title,

                            Description =
                                x.Description ?? string.Empty,

                            Icon =
                                x.Icon ?? string.Empty,

                            DisplayOrder =
                                x.DisplayOrder
                        })
                        .ToList()
                };

                // =========================================================
                // 12. SUCCESS
                // =========================================================

                return new ApiResponse<AboutUsResponseDto>
                {
                    Success = true,
                    Message =
                        "AboutUs and Features updated successfully.",
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
                    Message =
                        ex.InnerException?.Message ??
                        ex.Message,
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
                        Message = "Invalid FarmHouse."
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
                        Message = "Unauthorized."
                    };
                }


               
                // 3. Check FarmHouse
                

                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
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
                        Message = "You are not authorized to access this FarmHouse."
                    };
                }


               
                // 5. Get Existing AboutUs
                

                var aboutUs = await _aboutUsRepository
                    .GetAboutUsAsync(aboutUsId);

                if (aboutUs == null || aboutUs.IsDelete)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs not found."
                    };
                }


               
                // 6. Check AboutUs belongs to FarmHouse
               
                if (aboutUs.FarmHouseId != farmHouseId)
                {
                    return new ApiResponse<AboutUsResponseDto>
                    {
                        Success = false,
                        Message = "AboutUs does not belong to this FarmHouse."
                    };
                }


                // 7. Delete All Features First
              
                foreach (var feature in aboutUs.Features
                             .Where(x => !x.IsDelete))
                {
                    feature.IsDelete = true;

                    await _aboutUsRepository
                        .UpdateFeatureAsync(feature);
                }


                
                // 8. Delete AboutUs
               
                aboutUs.IsDelete = true;

                aboutUs.ModifyBy = userId;

                aboutUs.ModifyDate = DateTime.UtcNow;

                await _aboutUsRepository
                    .UpdateAboutUsAsync(aboutUs);


               
                // 9. Save Changes
               

                await _unitOfWork.SaveChangesAsync();


               
                // 10. Create Response DTO
               

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

                            Description = x.Description
                                ?? string.Empty,

                            Icon = x.Icon
                                ?? string.Empty,

                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList()
                };


                // 11. Success Response
              

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

                    Message = "Something went wrong while deleting AboutUs."
                };
            }
        }
    }
}
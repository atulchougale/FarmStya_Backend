using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Public;

namespace FarmStay.Application.Interfaces.Services.Public
{
    public interface IPublicPageService
    {
        Task<ApiResponse<List<AmenityResponseDto>>> GetAmenityAsync();

        Task<ApiResponse<List<AmenityResponseDto>>> GetCarouselAsync();

        Task<ApiResponse<List<FeedbackResponseDto>>> GetFeedbackAsync();

        Task<ApiResponse<List<GalleryResponseDto>>> GetGalleryAsync();
    }
}
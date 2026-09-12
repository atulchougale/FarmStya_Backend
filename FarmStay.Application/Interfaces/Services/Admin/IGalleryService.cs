using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Admin;

public interface IGalleryService
{
    Task<ApiResponse<GalleryResponseDto>> SaveGalleryAsync( GalleryRequestDto dto);

    Task<ApiResponse<GalleryResponseDto>> GetGalleryByIdAsync(int imageId);

    Task<ApiResponse<List<GalleryResponseDto>>> GetAllGalleryAsync();

    Task<ApiResponse<GalleryResponseDto>> UpdateGalleryAsync(GalleryRequestDto dto);

    Task<ApiResponse<GalleryResponseDto>> DeleteGalleryAsync(int imageId);

    Task<ApiResponse<List<string>>> GetCategoryListAsync();
}
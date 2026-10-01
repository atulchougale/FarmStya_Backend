using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Public;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IPublicPageRepository
    {
        Task<List<AmenityResponseDto>> GetAmenitiesAsync(int farmHouseId);

        Task<List<AmenityResponseDto>> GetCarsolesAsync(int farmHouseId);

        Task<List<FeedbackResponseDto>> GetFeedbacksAsync(int farmHouseId);

        Task<List<GalleryResponseDto>> GetGalleryAync(int farmHouseId);
    }
}
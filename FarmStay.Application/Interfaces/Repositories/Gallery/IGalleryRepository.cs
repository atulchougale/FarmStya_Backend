using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IGalleryRepository
    {
        Task AddAsync(Gallery gallery);

        Task<Gallery?> GetByIdAsync(int imageId);

        Task<Gallery?> GetByImageNameAsync(int farmHouseId, string imageName);

        Task<List<Gallery>> GetAllAsync(int farmHouseId);

        Task UpdateAsync(Gallery gallery);

        Task<List<string>> GetCategoryListAsync(int farmHouseId);
    }
}

using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IFarmHousePricingRepository
    {
        Task<List<FarmHousePricing>> GetActivePricingByFarmHouseIdAsync(
            int farmHouseId);
    }
}
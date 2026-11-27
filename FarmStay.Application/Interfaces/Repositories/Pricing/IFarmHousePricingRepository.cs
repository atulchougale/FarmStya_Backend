using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IFarmHousePricingRepository
    {
        Task<List<FarmStay.Application.DTOs.Booking.PricingEntry>> GetEntriesAsync(int farmHouseId);
        Task<FarmStay.Application.DTOs.Booking.PricingChangeResult> ChangeEntryAsync(int farmHouseId, long userId, int? pricingId,
            FarmStay.Application.DTOs.Booking.PricingEntryRequest? request);
        Task SaveRatesAsync(int farmHouseId, long userId, FarmStay.Application.DTOs.Booking.PricingRates rates);
        Task<List<FarmHousePricing>> GetActivePricingByFarmHouseIdAsync(
            int farmHouseId);
    }
}
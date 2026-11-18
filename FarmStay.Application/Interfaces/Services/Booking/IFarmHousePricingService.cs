using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking
{
    public interface IFarmHousePricingService
    {
        Task<ApiResponse<List<FarmHousePricingResponseDto>>> GetActivePricingAsync();
        Task<List<PricingEntry>> GetEntriesAsync(int farmHouseId);
        Task<PricingChangeResult> ChangeEntryAsync(int farmHouseId, long userId, int? pricingId, PricingEntryRequest? request);
        Task<ApiResponse<PricingRates>> GetManagementPricingAsync(int farmHouseId);
        Task<ApiResponse<PricingRates>> SaveManagementPricingAsync(int farmHouseId, long userId, PricingRates rates);
        Task<ApiResponse<BookingQuoteResponse>> GetQuoteAsync(BookingQuoteRequest request);
    }
}

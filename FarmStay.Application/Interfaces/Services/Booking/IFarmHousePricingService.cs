using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking
{
    public interface IFarmHousePricingService
    {
        Task<ApiResponse<List<FarmHousePricingResponseDto>>> GetActivePricingAsync();
    }
}
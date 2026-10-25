
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Booking
{
    public class FarmHousePricingService : IFarmHousePricingService
    {
        private readonly IFarmHousePricingRepository _pricingRepository;
        private readonly ILogger<FarmHousePricingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FarmHousePricingService(
            IFarmHousePricingRepository pricingRepository,
            ILogger<FarmHousePricingService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _pricingRepository = pricingRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<List<FarmHousePricingResponseDto>>>
            GetActivePricingAsync()
        {
            var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                .Request
                .Headers["FarmHouseId"]
                .FirstOrDefault();

            if (!int.TryParse(farmHouseIdHeader, out int farmHouseId)
                || farmHouseId <= 0)
            {
                _logger.LogWarning(
                    "FarmHouseId header is missing or invalid.");

                return new ApiResponse<List<FarmHousePricingResponseDto>>
                {
                    Success = false,
                    Message = "Invalid FarmHouse.",
                    Data = null
                };
            }

            _logger.LogInformation(
                "GetActivePricing request received. FarmHouseId: {FarmHouseId}",
                farmHouseId);

            try
            {
                var pricingRecords =
                    await _pricingRepository
                        .GetActivePricingByFarmHouseIdAsync(farmHouseId);

                var response = pricingRecords
                    .Select(p => new FarmHousePricingResponseDto
                    {
                        PricingId = p.PricingId,
                        PricingKey = p.PricingKey,
                        PricingName = p.PricingName,
                        Price = p.Price,
                        CurrencyCode = p.CurrencyCode
                    })
                    .ToList();

                _logger.LogInformation(
                    "Pricing retrieved successfully. FarmHouseId: {FarmHouseId}, Count: {Count}",
                    farmHouseId,
                    response.Count);

                return new ApiResponse<List<FarmHousePricingResponseDto>>
                {
                    Success = true,
                    Message = response.Count > 0
                        ? "Pricing retrieved successfully."
                        : "No active pricing records found.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving pricing. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<List<FarmHousePricingResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving pricing.",
                    Data = null
                };
            }
        }
    }
}

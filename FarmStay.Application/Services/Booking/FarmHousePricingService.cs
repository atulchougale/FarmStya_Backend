
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

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

        public Task<List<PricingEntry>> GetEntriesAsync(int farmHouseId) => _pricingRepository.GetEntriesAsync(farmHouseId);

        public async Task<PricingChangeResult> ChangeEntryAsync(int farmHouseId, long userId, int? pricingId, PricingEntryRequest? request)
        {
            if (request != null)
            {
                var key = request.PricingKey?.Trim().ToUpperInvariant() ?? "";
                var name = request.PricingName?.Trim() ?? "";
                if (!System.Text.RegularExpressions.Regex.IsMatch(key, "^[A-Z][A-Z0-9_]{0,49}$") ||
                    name.Length is < 1 or > 150 || request.Price < 0 || request.Price > 100000000 ||
                    decimal.Round(request.Price, 2) != request.Price || (key == "BOOKING_DAILY" && request.Price <= 0))
                    return new(400, "Enter a valid pricing key, name and daily amount with up to two decimals.");
                request = request with { PricingKey = key, PricingName = name };
            }
            var result = await _pricingRepository.ChangeEntryAsync(farmHouseId, userId, pricingId, request);
            _logger.LogInformation("Pricing change completed. FarmHouseId: {FarmHouseId}, UserId: {UserId}, PricingId: {PricingId}, Action: {Action}, Result: {Status}",
                farmHouseId, userId, pricingId, request == null ? "Delete" : pricingId == null ? "Create" : "Update", result.Status);
            return result;
        }

        public async Task<ApiResponse<PricingRates>> GetManagementPricingAsync(int farmHouseId)
        {
            var rates = await _pricingRepository.GetActivePricingByFarmHouseIdAsync(farmHouseId);
            decimal Rate(string key) => rates.SingleOrDefault(r => r.PricingKey == key)?.Price ?? 0;
            return new() { Success = true, Data = new(Rate("BOOKING_DAILY"), Rate("NORMAL_EVENT_DAILY"),
                Rate("DECORATION_EVENT_DAILY"), Rate("ADVANCE_DAILY")) };
        }

        public async Task<ApiResponse<PricingRates>> SaveManagementPricingAsync(int farmHouseId, long userId, PricingRates rates)
        {
            var amounts = new[] { rates.BookingDaily, rates.NormalEventDaily, rates.DecorationEventDaily, rates.AdvanceDaily };
            if (amounts.Any(value => value < 0 || value > 100000000 || decimal.Round(value, 2) != value) ||
                rates.BookingDaily <= 0 || rates.AdvanceDaily > rates.BookingDaily)
                return new() { Success = false, Message = "Enter valid daily rates with up to two decimals. Booking rate must be positive and advance cannot exceed it." };
            await _pricingRepository.SaveRatesAsync(farmHouseId, userId, rates);
            _logger.LogInformation("Farmhouse daily pricing updated. FarmHouseId: {FarmHouseId}, UserId: {UserId}", farmHouseId, userId);
            return new() { Success = true, Message = "Daily pricing saved successfully.", Data = rates };
        }

        public async Task<ApiResponse<BookingQuoteResponse>> GetQuoteAsync(BookingQuoteRequest request)
        {
            var stayDays = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
            var eventDates = request.EventDates ?? [];
            var eventKey = request.BookingType switch
            {
                "NormalEvent" => "NORMAL_EVENT_DAILY",
                "DecorationEvent" => "DECORATION_EVENT_DAILY",
                _ => null
            };

            if (request.CheckInDate == default || stayDays <= 0 ||
                (eventKey == null ? request.BookingType != "BookingOnly" || eventDates.Length != 0 : eventDates.Length == 0) ||
                eventDates.Distinct().Count() != eventDates.Length ||
                eventDates.Any(date => date < request.CheckInDate || date >= request.CheckOutDate))
                return QuoteError("Select a valid stay and event dates within that stay.");

            var pricing = await GetActivePricingAsync();
            if (!pricing.Success)
                return QuoteError(pricing.Message);
            if (pricing.Data is not { } activeRates)
                return QuoteError("Booking pricing is unavailable.");

            var keys = new List<string> { "BOOKING_DAILY", "ADVANCE_DAILY" };
            if (eventKey != null)
                keys.Add(eventKey);

            var rates = activeRates.Where(rate => keys.Contains(rate.PricingKey))
                .OrderBy(rate => rate.PricingKey, StringComparer.Ordinal).ToArray();
            if (rates.Select(rate => rate.PricingKey).Distinct().Count() != keys.Count ||
                rates.Length != keys.Count || rates.Any(rate => rate.Price < 0) ||
                string.IsNullOrWhiteSpace(rates[0].CurrencyCode) ||
                rates.Any(rate => rate.CurrencyCode != rates[0].CurrencyCode))
                return QuoteError("Booking pricing is unavailable.");

            var stayRate = rates.Single(rate => rate.PricingKey == "BOOKING_DAILY").Price;
            var advanceRate = rates.Single(rate => rate.PricingKey == "ADVANCE_DAILY").Price;
            var eventRate = eventKey == null ? 0 : rates.Single(rate => rate.PricingKey == eventKey).Price;
            var stay = stayRate * stayDays;
            var events = eventRate * eventDates.Length;
            var total = stay + events;
            var advance = advanceRate * stayDays;
            if (advance > total)
                return QuoteError("The configured advance exceeds the booking total.");

            var snapshot = string.Join("|", rates.Select(rate =>
                FormattableString.Invariant($"{rate.PricingKey}:{rate.Price:F2}:{rate.CurrencyCode}")));
            var version = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot)));

            return new ApiResponse<BookingQuoteResponse>
            {
                Success = true,
                Data = new BookingQuoteResponse(stayDays, eventDates.Length, stayRate, eventRate,
                    advanceRate, stay, events, total, advance, total - advance, rates[0].CurrencyCode, version)
            };
        }

        private static ApiResponse<BookingQuoteResponse> QuoteError(string message) =>
            new() { Success = false, Message = message };

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

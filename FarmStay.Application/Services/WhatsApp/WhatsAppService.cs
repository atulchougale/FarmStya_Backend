using FarmStay.Application.Configurations;
using FarmStay.Application.DTOs.WhatsApp;
using FarmStay.Application.Interfaces.Services.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FarmStay.Infrastructure.Services.WhatsApp
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppSettings> options,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendOtpAsync(
            string mobileNumber,
            string otp)
        {
            var message =
                $"Your FarmStay verification OTP is {otp}. It is valid for 5 minutes.";

            return await SendMessageAsync(
                mobileNumber,
                message);
        }

        public async Task<bool> SendMessageAsync(
            string mobileNumber,
            string message)
        {
            if (!_settings.IsEnabled)
            {
                _logger.LogInformation(
                    "WhatsApp service is disabled.");

                return true;
            }

            try
            {
                var normalizedNumber =
                    mobileNumber.StartsWith("91")
                        ? mobileNumber
                        : $"91{mobileNumber}";

                var request = new WhatsAppMessageRequest
                {
                    Recipient = normalizedNumber,
                    Message = message
                };

                using var httpRequest =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        _settings.SendMessageEndpoint);

                httpRequest.Content =
                    JsonContent.Create(request);

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _settings.GatewayToken);

                var response =
                    await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "WhatsApp Gateway returned HTTP {StatusCode}",
                        response.StatusCode);

                    return false;
                }

                var result =
                    await response.Content
                        .ReadFromJsonAsync<WhatsAppResponseDto>();

                if (result == null)
                {
                    _logger.LogError(
                        "Invalid or empty response received from WhatsApp Gateway.");

                    return false;
                }

                if (!result.Success)
                {
                    _logger.LogError(
                        "WhatsApp Gateway failed. Message: {Message}",
                        result.Message);

                    return false;
                }

                _logger.LogInformation(
                    "WhatsApp message sent successfully to {Recipient}.",
                    mobileNumber);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while sending WhatsApp message to {Recipient}.",
                    mobileNumber);

                return false;
            }
        }
    }
}
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.BackgroundServices
{
    public class WhatsAppBackgroundService : BackgroundService
    {
        private readonly IWhatsAppQueue _whatsAppQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WhatsAppBackgroundService> _logger;


        public WhatsAppBackgroundService(
            IWhatsAppQueue whatsAppQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<WhatsAppBackgroundService> logger)
            {
                _whatsAppQueue = whatsAppQueue;
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "WhatsApp Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _whatsAppQueue.DequeueAsync(stoppingToken);

                    _logger.LogInformation(
                        "Processing WhatsApp message for {MobileNumber}",
                        job.MobileNumber);

                    using var scope = _scopeFactory.CreateScope();

                    var whatsAppService =
                        scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

                    var isSent = await whatsAppService.SendMessageAsync(
                        job.MobileNumber,
                        job.Message);

                    if (isSent)
                    {
                        _logger.LogInformation(
                            "WhatsApp message sent successfully to {MobileNumber}",
                            job.MobileNumber);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "WhatsApp message failed for {MobileNumber}",
                            job.MobileNumber);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation(
                        "WhatsApp Background Service stopped.");

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process WhatsApp message from queue.");
                }
            }

            _logger.LogInformation(
                "WhatsApp Background Service exited.");
        }
    }
}

using FarmStay.Application.Interfaces.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmStay.Infrastructure.BackgroundServices
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;


        public EmailBackgroundService(
            IEmailQueue emailQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundService> logger)
            {
                _emailQueue = emailQueue;
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Email Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
               
                try
                {
                    var job = await _emailQueue.DequeueAsync(stoppingToken);

                    _logger.LogInformation(
                        "Processing email for {Email}",
                        job.To);

                    using var scope = _scopeFactory.CreateScope();

                    var emailService =
                        scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendEmailAsync(
                        job.To,
                        job.Subject,
                        job.HtmlBody,
                        job.IsHtml);

                    _logger.LogInformation(
                        "Email sent successfully to {Email}",
                        job.To);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation(
                        "Email Background Service stopped.");

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, "Failed to process email for {Email}.");
                }
            }

            _logger.LogInformation(
                "Email Background Service exited.");
        }
    }
}

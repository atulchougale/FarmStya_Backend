using FarmStay.Application.BackgroundJobs;

namespace FarmStay.Application.Interfaces.Common
{
    public interface IEmailQueue
    {
        void Enqueue(EmailJob job);

        Task<EmailJob> DequeueAsync(
            CancellationToken cancellationToken
        );
    }
}
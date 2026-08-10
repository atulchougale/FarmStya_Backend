using FarmStay.Application.BackgroundJobs;

namespace FarmStay.Application.Interfaces.Common
{
    public interface IWhatsAppQueue
    {
        void Enqueue(WhatsAppJob job);

        Task<WhatsAppJob> DequeueAsync(
            CancellationToken cancellationToken
        );
    }
}
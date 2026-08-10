using System.Threading.Channels;
using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Interfaces.Common;

namespace FarmStay.Infrastructure.Services.Communication
{
    public class WhatsAppQueue : IWhatsAppQueue
    {
        private readonly Channel<WhatsAppJob> _queue;

        public WhatsAppQueue()
        {
            _queue = Channel.CreateUnbounded<WhatsAppJob>();
        }

        public void Enqueue(WhatsAppJob job)
        {
            if (!_queue.Writer.TryWrite(job))
            {
                throw new InvalidOperationException(
                    "Unable to enqueue WhatsApp message."
                );
            }
        }

        public async Task<WhatsAppJob> DequeueAsync(
            CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(
                cancellationToken
            );
        }
    }
}
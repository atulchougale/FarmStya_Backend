using System.Threading.Channels;
using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Interfaces.Common;

namespace FarmStay.Infrastructure.Services.Communication
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailJob> _queue;

        public EmailQueue()
        {
            _queue = Channel.CreateUnbounded<EmailJob>();
        }

        public void Enqueue(EmailJob job)
        {
            if (!_queue.Writer.TryWrite(job))
            {
                throw new InvalidOperationException(
                    "Unable to enqueue email."
                );
            }
        }

        public async Task<EmailJob> DequeueAsync(
            CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(
                cancellationToken
            );
        }
    }
}
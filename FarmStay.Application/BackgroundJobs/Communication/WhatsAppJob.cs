namespace FarmStay.Application.BackgroundJobs
{
    public class WhatsAppJob
    {
        public string MobileNumber { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
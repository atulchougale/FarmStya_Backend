namespace FarmStay.Application.DTOs.WhatsApp
{
    public class WhatsAppMessageRequest
    {
        public string Recipient { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
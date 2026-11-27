namespace FarmStay.Application.DTOs.WhatsApp
{
    public class WhatsAppResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public object? Data { get; set; }
    }
}
namespace FarmStay.Application.Configurations
{
    public class WhatsAppSettings
    {
        public const string SectionName = "WhatsApp";

        public string BaseUrl { get; set; } = string.Empty;

        public string SendMessageEndpoint { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public string GatewayToken { get; set; } = string.Empty;
    }
}

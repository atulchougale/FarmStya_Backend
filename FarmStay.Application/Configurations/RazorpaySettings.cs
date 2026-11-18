namespace FarmStay.Application.Configurations;

public sealed class RazorpaySettings
{
    public const string SectionName = "Razorpay";

    public bool Enabled { get; set; }
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

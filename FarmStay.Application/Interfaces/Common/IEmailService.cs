namespace FarmStay.Application.Interfaces.Common
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            bool isHtml = true
        );
    }
}
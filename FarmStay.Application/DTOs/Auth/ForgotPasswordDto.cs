using FarmStay.Domain.Enums;

namespace FarmStay.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        public ForgotPasswordMethod Method { get; set; }

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
    }
}
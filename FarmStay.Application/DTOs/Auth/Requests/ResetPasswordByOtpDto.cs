namespace FarmStay.Application.DTOs.Auth
{
    public class ResetPasswordOtpDto
    {
        public string MobileNumber { get; set; } = string.Empty;

        public string OtpCode { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
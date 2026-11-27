namespace FarmStay.Application.DTOs.Auth
{
    public class VerifyLoginOtpRequestDto
    {
        public string MobileNumber { get; set; } = string.Empty;

        public string OtpCode { get; set; } = string.Empty;
    }
}
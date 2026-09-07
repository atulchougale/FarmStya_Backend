namespace FarmStay.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public int? FarmHouseId { get; set; }

        public bool IsEmailVerificationSent { get; set; }

        public bool IsMobileOtpSent { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
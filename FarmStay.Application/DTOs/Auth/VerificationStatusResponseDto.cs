namespace FarmStay.Application.DTOs.Auth
{
    public class VerificationStatusResponseDto
    {
        public bool HasEmail { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }
    }
}
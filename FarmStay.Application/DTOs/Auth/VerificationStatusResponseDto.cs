namespace FarmStay.Application.DTOs.Auth
{
    public class VerificationStatusResponseDto
    {
        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }
    }
}
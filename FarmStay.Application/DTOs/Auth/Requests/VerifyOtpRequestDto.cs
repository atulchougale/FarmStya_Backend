namespace FarmStay.Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public int FarmHouseId { get; set; }

        public int UserId { get; set; }

        public string OtpCode { get; set; } = string.Empty;
    }
}
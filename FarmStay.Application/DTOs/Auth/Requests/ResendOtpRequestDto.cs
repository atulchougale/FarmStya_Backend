namespace FarmStay.Application.DTOs.Auth
{
    public class ResendOtpRequestDto
    {
        public int FarmHouseId { get; set; }

        public int UserId { get; set; }
    }
}
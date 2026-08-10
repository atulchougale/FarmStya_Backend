namespace FarmStay.Application.DTOs.Auth
{
    public class VerifyEmailRequestDto
    {
        public int FarmHouseId { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
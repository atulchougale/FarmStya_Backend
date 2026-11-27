namespace FarmStay.Application.DTOs.Auth
{
    public class ResetPasswordByEmailDto
    {
        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
namespace FarmStay.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        // JWT Token
        public string Token { get; set; } = string.Empty;


    // Refresh Token
        public string RefreshToken { get; set; } = string.Empty;

        // User Information
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Current FarmHouse Context
        public int FarmHouseId { get; set; }

        public string FarmHouseName { get; set; } = string.Empty;

        // Current Role
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        // Membership Information
        public bool IsOwner { get; set; }
    }

}

namespace FarmStay.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        // JWT Access Token
        public string AccessToken { get; set; } = string.Empty;

        // Refresh Token
        public string RefreshToken { get; set; } = string.Empty;

        // User Information
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        // Current FarmHouse Context
        public int FarmHouseId { get; set; }

        public string FarmHouseName { get; set; } = string.Empty;

        // Current Role
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        // Membership Information
        public bool IsOwner { get; set; }

        // Verification Status
        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool RequiresVerification { get; set; }
    }
}
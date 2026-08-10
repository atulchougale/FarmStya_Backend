namespace FarmStay.Application.DTOs.Auth
{
    public class ProfileDto
    {
        // User Information
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        // Current FarmHouse
        public int FarmHouseId { get; set; }

        public string FarmHouseName { get; set; } = string.Empty;

        // Current Role
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        // Membership
        public bool IsOwner { get; set; }
    }
}
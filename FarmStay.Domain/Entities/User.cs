using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        // ================= Relationships =================

        [Required]
        public int? FarmHouseId { get; set; }

        // ================= Personal Information =================

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // ================= Verification =================

        public bool IsEmailVerified { get; set; } = false;

        public bool IsMobileVerified { get; set; } = false;

        [MaxLength(200)]
        public string? EmailVerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        // ================= Status =================

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        // One User -> One or More Memberships (Future Safe)
        public ICollection<UserMembership> UserMemberships { get; set; }
            = new List<UserMembership>();

        // Properties owned by this user
        public ICollection<Property> Properties { get; set; }
            = new List<Property>();

        // Bookings made by this user
        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();

        // Refresh Tokens
        public ICollection<UserRefreshToken> UserRefreshTokens { get; set; }
            = new List<UserRefreshToken>();

        // OTP History
        public ICollection<UserOtp> UserOtps { get; set; }
            = new List<UserOtp>();
    }
}
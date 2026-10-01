using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FarmStay.Domain.Enums;

namespace FarmStay.Domain.Entities
{
    public class UserOtp
    {
        [Key]
        public int UserOtpId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(10)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(6)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public OtpPurpose Purpose { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        // Number of wrong OTP entries; the OTP is invalidated after too many failures
        public int FailedAttempts { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
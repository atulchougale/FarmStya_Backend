using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class UserRefreshToken
    {
        [Key]
        public int UserRefreshTokenId { get; set; }

        // ================= Relationships =================

        [Required]
        public int UserId { get; set; }

        // ================= Token Information =================

        [Required]
        [MaxLength(1000)]
        public string RefreshTokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        // ================= Device Information =================

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [MaxLength(100)]
        public string? Browser { get; set; }

        [MaxLength(100)]
        public string? OperatingSystem { get; set; }

        [MaxLength(100)]
        public string? IPAddress { get; set; }

        // ================= Token Rotation =================

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedDate { get; set; }

        public int? ReplacedByTokenId { get; set; }

        // ================= Status =================

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

        [ForeignKey(nameof(ReplacedByTokenId))]
        public UserRefreshToken? ReplacedByToken { get; set; }

        public ICollection<UserRefreshToken> ChildTokens { get; set; }
            = new List<UserRefreshToken>();
    }
}
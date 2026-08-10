using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class UserMembership
    {
        [Key]
        public int UserMembershipId { get; set; }

        // ================= Relationships =================

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public int RoleId { get; set; }

        // Default FarmHouse for user (Future Ready)
        public bool IsDefault { get; set; } = false;

        // ================= Status =================

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ================= Activity =================

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }
    }
}
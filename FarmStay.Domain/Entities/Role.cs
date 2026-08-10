using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        // Modules assigned to this role
        public ICollection<RoleModule> RoleModules { get; set; }
            = new List<RoleModule>();

        public ICollection<UserMembership> UserMemberships { get; set; }
            = new List<UserMembership>();
    }
}
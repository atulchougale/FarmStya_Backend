using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class RoleModulePermission
    {
        [Key]
        public int RoleModulePermissionId { get; set; }

        [Required]
        public int RoleModuleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        // Permission Status
        public bool IsAllowed { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        [ForeignKey(nameof(RoleModuleId))]
        public RoleModule? RoleModule { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public Permission? Permission { get; set; }
    }
}
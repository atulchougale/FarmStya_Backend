using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsSystemPermission { get; set; } = true;

        public bool IsActive { get; set; } = true;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        public ICollection<RoleModulePermission> RoleModulePermissions { get; set; }
            = new List<RoleModulePermission>();
    }
}
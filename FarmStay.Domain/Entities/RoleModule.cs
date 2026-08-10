using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class RoleModule
    {
        [Key]
        public int RoleModuleId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        public bool IsActive { get; set; } = true;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        public Role? Role { get; set; }

        public Module? Module { get; set; }

        public ICollection<RoleModulePermission> RoleModulePermissions { get; set; }
            = new List<RoleModulePermission>();
    }
}
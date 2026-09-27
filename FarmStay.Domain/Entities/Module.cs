using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(200)]
        public string? Route { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        public ICollection<RoleModule> RoleModules { get; set; }
            = new List<RoleModule>();

        public ICollection<FarmHouseModule> FarmHouseModules { get; set; }
    = new List<FarmHouseModule>();
    }
}
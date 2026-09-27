using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FarmHouseModule
    {
        [Key]
        public int FarmHouseModuleId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }


        // Navigation Properties

        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(ModuleId))]
        public Module? Module { get; set; }
    }
}
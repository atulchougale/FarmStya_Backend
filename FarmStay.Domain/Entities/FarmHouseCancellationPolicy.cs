
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FarmHouseCancellationPolicy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public int PolicyVersion { get; set; }

        public bool PendingCancelAllowed { get; set; }

        public bool ApprovedCancelAllowed { get; set; }

        public bool ConfirmedAdvanceRefundable { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string PolicyText { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation property
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }
    }
}

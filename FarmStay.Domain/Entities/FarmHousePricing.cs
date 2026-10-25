
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FarmHousePricing
    {
        [Key]
        public int PricingId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PricingKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PricingName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "char(3)")]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class SubscriptionPlan
    {
        [Key]
        public int SubscriptionPlanId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        // Duration in days (e.g., 30, 90, 365)
        [Required]
        public int Duration { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // Navigation
        public ICollection<FarmHouse> FarmHouses { get; set; }
            = new List<FarmHouse>();
    }
}
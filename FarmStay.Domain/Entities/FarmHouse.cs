using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FarmHouse
    {
        [Key]
        public int FarmHouseId { get; set; }

    // ================= Owner =================
      
        public int? OwnerUserId { get; set; }

        // ================= Basic Information =================

        [Required]
        [MaxLength(150)]
        public string FarmHouseName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DomainName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SubDomain { get; set; }

        public bool IsPrimaryDomain { get; set; } = false;

        [MaxLength(250)]
        public string? TagLine { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // ================= Address =================

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Village { get; set; }

        [Required]
        [MaxLength(100)]
        public string Taluka { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Pincode { get; set; }

        // ================= Contact =================

        [Required]
        [MaxLength(100)]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? AlternateMobileNumber { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        // ================= Media =================

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(500)]
        public string? GoogleMapUrl { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        // ================= Website =================

        public bool IsWebsitePublished { get; set; } = true;

        // ================= Subscription =================

        public int? SubscriptionPlanId { get; set; }

        public DateTime? SubscriptionStartDate { get; set; }

        public DateTime? SubscriptionEndDate { get; set; }

        public bool IsSubscriptionActive { get; set; } = false;

        // ================= Status =================

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ================= Audit =================

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // ================= Navigation =================

        [ForeignKey(nameof(OwnerUserId))]
        public User? OwnerUser { get; set; }

        public SubscriptionPlan? SubscriptionPlan { get; set; }

        public ICollection<UserMembership> UserMemberships { get; set; }
            = new List<UserMembership>();

        public ICollection<User> Users { get; set; }
            = new List<User>();
        
        public ICollection<Property> Properties { get; set; }
            = new List<Property>();

        public ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();

        public ICollection<FeedBack> FeedBacks { get; set; } = new List<FeedBack>();

        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    }

}

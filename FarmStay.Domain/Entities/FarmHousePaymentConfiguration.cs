
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FarmHousePaymentConfiguration
    {
        [Key]
        public long FarmHousePaymentConfigurationId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Environment { get; set; } = "Test";

        [Required]
        [MaxLength(150)]
        public string MerchantAccountId { get; set; } = string.Empty;

        // Reference to encrypted credentials in secure storage.
        [MaxLength(500)]
        public string? CredentialsSecretReference { get; set; }

        [MaxLength(100)]
        public string? WebhookSecretReference { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "INR";

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


using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class FarmHousePricingConfiguration
        : IEntityTypeConfiguration<FarmHousePricing>
    {
        public void Configure(EntityTypeBuilder<FarmHousePricing> builder)
        {
            // Table
            builder.ToTable("FarmHousePricing");

            // Primary Key
            builder.HasKey(p => p.PricingId);

            builder.Property(p => p.PricingId)
                .ValueGeneratedOnAdd();

            // Tenant
            builder.Property(p => p.FarmHouseId)
                .IsRequired();

            builder.HasOne(p => p.FarmHouse)
                .WithMany()
                .HasForeignKey(p => p.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pricing Key
            builder.Property(p => p.PricingKey)
                .HasMaxLength(50)
                .IsUnicode()
                .IsRequired();

            // Display Name
            builder.Property(p => p.PricingName)
                .HasMaxLength(150)
                .IsUnicode()
                .IsRequired();

            // Rate
            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Currency
            builder.Property(p => p.CurrencyCode)
                .HasColumnType("char(3)")
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("INR")
                .IsRequired();

            // Status
            builder.Property(p => p.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(p => p.IsDelete)
                .HasDefaultValue(false)
                .IsRequired();

            // Audit
            builder.Property(p => p.CreatedBy)
                .IsRequired();

            builder.Property(p => p.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(p => p.ModifiedBy)
                .IsRequired(false);

            builder.Property(p => p.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Prevent duplicate active pricing keys for the same farmhouse.
            builder.HasIndex(p => new
            {
                p.FarmHouseId,
                p.PricingKey
            })
                .IsUnique()
                .HasFilter("[IsActive] = 1 AND [IsDelete] = 0");
        }
    }
}

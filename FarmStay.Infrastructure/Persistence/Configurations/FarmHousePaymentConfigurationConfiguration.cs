
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class FarmHousePaymentConfigurationConfiguration
        : IEntityTypeConfiguration<FarmHousePaymentConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<FarmHousePaymentConfiguration> builder)
        {
            builder.ToTable("FarmHousePaymentConfigurations");

            builder.HasKey(x => x.FarmHousePaymentConfigurationId);

            builder.Property(x => x.FarmHousePaymentConfigurationId)
                .UseIdentityColumn();

            builder.Property(x => x.ProviderName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Environment)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.MerchantAccountId)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.CredentialsSecretReference)
                .HasMaxLength(500);

            builder.Property(x => x.WebhookSecretReference)
                .HasMaxLength(100);

            builder.Property(x => x.CurrencyCode)
                .HasColumnType("char(3)")
                .HasDefaultValue("INR")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(x => x.ModifiedDate)
                .HasColumnType("datetime2(0)");

            builder.HasOne(x => x.FarmHouse)
                .WithMany()
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.ProviderName,
                x.MerchantAccountId
            }).IsUnique();

            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}

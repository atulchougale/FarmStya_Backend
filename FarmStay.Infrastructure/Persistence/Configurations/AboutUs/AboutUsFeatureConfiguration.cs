using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class AboutUsFeatureConfiguration
        : IEntityTypeConfiguration<AboutUsFeature>
    {
        public void Configure(
            EntityTypeBuilder<AboutUsFeature> builder)
        {
            builder.ToTable("AboutUsFeatures");

            builder.HasKey(x => x.FeatureId);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Icon)
                .HasMaxLength(200);

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.AboutUsId);
        }
    }
}
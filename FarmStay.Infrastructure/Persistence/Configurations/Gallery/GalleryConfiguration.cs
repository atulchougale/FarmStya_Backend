using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class GalleryConfiguration : IEntityTypeConfiguration<Gallery>
    {
        public void Configure(EntityTypeBuilder<Gallery> builder)
        {
            // Table
            builder.ToTable("Galleries");

            // Primary Key
            builder.HasKey(x => x.ImageId);

            // Properties
            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ImageName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.FarmHouseId)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.ModifyBy)
                .IsRequired();

            builder.Property(x => x.ModifyDate)
                .IsRequired();

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            // Foreign Key
            builder.HasOne(x => x.FarmHouse)
               .WithMany(x => x.Galleries)
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index
            builder.HasIndex(x => x.FarmHouseId);
        }
    }
}
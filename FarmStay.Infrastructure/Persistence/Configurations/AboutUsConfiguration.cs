using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class AboutUsConfiguration : IEntityTypeConfiguration<AboutUs>
    {
        public void Configure(EntityTypeBuilder<AboutUs> builder)
        {
           
           //table

            builder.ToTable("AboutUs");

            
            // PRIMARY KEY
           

            builder.HasKey(x => x.AboutUsId);

           
            // PROPERTIES
           

            builder.Property(x => x.HeroTitle)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.HeroSubtitle)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.HeroImageUrl)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.StoryTitle)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.StoryDescription)
                   .HasMaxLength(2000);

            builder.Property(x => x.FarmHouseId)
                   .IsRequired();

            builder.Property(x => x.IsDelete)
                   .HasDefaultValue(false);

           
            //AboutUs
            

            builder.HasOne(x => x.FarmHouse)
                   .WithOne(x => x.AboutUs)
                   .HasForeignKey<AboutUs>(x => x.FarmHouseId)
                   .OnDelete(DeleteBehavior.Restrict);

           
            

            builder.HasIndex(x => x.FarmHouseId)
                   .IsUnique()
                   .HasFilter("[IsDelete] = 0");
        }
    }
}
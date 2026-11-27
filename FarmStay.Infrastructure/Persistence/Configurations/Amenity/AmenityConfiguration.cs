using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            //table
            builder.ToTable("Amenities");

            //primary key

            // Primary Key
            builder.HasKey(x => x.ImageId);



            // Properties
            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);


            builder.Property(x => x.Title)
                .HasMaxLength(500);




            builder.Property(x => x.Description)
                .HasMaxLength(500);


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
               .WithMany(x => x.Amenities)
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index

            builder.HasIndex(x => x.FarmHouseId);
        }
    }
}
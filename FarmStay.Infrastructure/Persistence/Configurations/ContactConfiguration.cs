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
    public class ContactConfiguration : IEntityTypeConfiguration<ContactUs>
    {
        public void Configure(EntityTypeBuilder<ContactUs> builder)
        {
            //table
            builder.ToTable("ContactDetail");

            //primary key

            // Primary Key
            builder.HasKey(x => x.ContactId);




            // Properties
            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Email)
               .IsRequired();




            builder.Property(x => x.CreatedDate)
                .IsRequired();


            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            // Foreign Key
            builder.HasOne(x => x.FarmHouse)
               .WithMany(x => x.ContactUs)
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index
            builder.HasIndex(x => x.FarmHouseId);
        }
    }

}
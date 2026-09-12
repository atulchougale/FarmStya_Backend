using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<FeedBack>
    {
        public void Configure(EntityTypeBuilder<FeedBack> builder)
        {
            //table
            builder.ToTable("FeedBacks");

            //primary key

            // Primary Key
            builder.HasKey(x => x.FeedBackId);




            // Properties
            builder.Property(x => x.Review)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Rating)
                .HasPrecision(2, 1)
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
               .WithMany(x => x.FeedBacks)
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index
            builder.HasIndex(x => x.FarmHouseId);
        }
    }
}

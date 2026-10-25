
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class FarmHouseCancellationPolicyConfiguration
        : IEntityTypeConfiguration<FarmHouseCancellationPolicy>
    {
        public void Configure(
            EntityTypeBuilder<FarmHouseCancellationPolicy> builder)
        {
            builder.ToTable("FarmHouseCancellationPolicies");

            builder.HasKey(x => x.PolicyId);

            builder.Property(x => x.PolicyId)
                .UseIdentityColumn();

            builder.Property(x => x.PolicyText)
                .HasColumnType("nvarchar(max)")
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

            // A version number must not be duplicated within a farmhouse.
            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.PolicyVersion
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

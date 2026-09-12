using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        #region DbSets

        // Core
        public DbSet<User> Users { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<FarmHouse> FarmHouses { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }

        // RBAC
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleModule> RoleModules { get; set; }
        public DbSet<RoleModulePermission> RoleModulePermissions { get; set; }

        //Admin Pages

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Amenity> Amenities { get; set; }

        // Referesh Token
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region User

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.FarmHouseId, u.Email })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.FarmHouseId, u.MobileNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            #endregion

            #region SubscriptionPlan

            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(sp => sp.PlanName)
                .IsUnique();

            modelBuilder.Entity<SubscriptionPlan>()
                .Property(sp => sp.Price)
                .HasPrecision(18, 2);

            #endregion

            #region FarmHouse

            modelBuilder.Entity<FarmHouse>()
                .HasIndex(f => f.DomainName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<FarmHouse>()
                .HasIndex(f => f.SubDomain)
                .IsUnique()
                .HasFilter("[SubDomain] IS NOT NULL AND [IsDeleted] = 0");

            modelBuilder.Entity<FarmHouse>()
                .Property(f => f.Latitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<FarmHouse>()
                .Property(f => f.Longitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<FarmHouse>()
                .HasOne(f => f.OwnerUser)
                .WithMany()
                .HasForeignKey(f => f.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmHouse>()
                .HasOne(f => f.SubscriptionPlan)
                .WithMany(sp => sp.FarmHouses)
                .HasForeignKey(f => f.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region UserMembership

            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.User)
                .WithMany(u => u.UserMemberships)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.FarmHouse)
                .WithMany(f => f.UserMemberships)
                .HasForeignKey(um => um.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.Role)
                .WithMany(r => r.UserMemberships)
                .HasForeignKey(um => um.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMembership>()
                .HasIndex(um => new
                {
                    um.UserId,
                    um.FarmHouseId
                })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            #endregion

            #region Role

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            #endregion

            #region Module

            modelBuilder.Entity<Module>()
                .HasIndex(m => m.ModuleName)
                .IsUnique();

            #endregion

            #region Permission

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.PermissionName)
                .IsUnique();

            #endregion

            #region RoleModule

            modelBuilder.Entity<RoleModule>()
                .HasOne(rm => rm.Role)
                .WithMany(r => r.RoleModules)
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleModule>()
                .HasOne(rm => rm.Module)
                .WithMany(m => m.RoleModules)
                .HasForeignKey(rm => rm.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleModule>()
                .HasIndex(rm => new
                {
                    rm.RoleId,
                    rm.ModuleId
                })
                .IsUnique();

            #endregion

            #region RoleModulePermission

            modelBuilder.Entity<RoleModulePermission>()
                .HasOne(rmp => rmp.RoleModule)
                .WithMany(rm => rm.RoleModulePermissions)
                .HasForeignKey(rmp => rmp.RoleModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleModulePermission>()
                .HasOne(rmp => rmp.Permission)
                .WithMany(p => p.RoleModulePermissions)
                .HasForeignKey(rmp => rmp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleModulePermission>()
                .HasIndex(rmp => new
                {
                    rmp.RoleModuleId,
                    rmp.PermissionId
                })
                .IsUnique();

            #endregion

            #region Property

            modelBuilder.Entity<Property>()
                .Property(p => p.PricePerNight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Booking

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Property)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region BookingLog

            modelBuilder.Entity<BookingLog>()
                .HasOne(bl => bl.Booking)
                .WithMany(b => b.BookingLogs)
                .HasForeignKey(bl => bl.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region PropertyImage

            modelBuilder.Entity<PropertyImage>()
                .HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion



            modelBuilder.ApplyConfiguration(new UserRefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new UserOtpConfiguration());
            modelBuilder.ApplyConfiguration(new GalleryConfiguration());
            modelBuilder.ApplyConfiguration(new FeedbackConfiguration());
            modelBuilder.ApplyConfiguration(new AmenityConfiguration());
        }
    }
}
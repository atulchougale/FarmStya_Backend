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

        public DbSet<FarmHouseModule> farmHouseModules { get; set; }

        //Admin Pages   

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Amenity> Amenities { get; set; }

        public DbSet<ContactUs> ContactDetail { get; set; }

        public DbSet<AboutUs> AboutUs { get; set; }


        public DbSet<AboutUsFeature> AboutUsFeatures { get; set; }

        // Referesh Token
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<FarmHousePricing> FarmHousePricings { get; set; }

        public DbSet<BookingEventDate> BookingEventDates { get; set; }

        public DbSet<BookingDateReservation> BookingDateReservations{ get; set; }

        public DbSet<BookingStayDate> BookingStayDates { get; set; }

        public DbSet<BookingDocument> BookingDocuments { get; set; }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<BookingReceipt> BookingReceipts { get; set; }

        public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }

        public DbSet<CheckoutDiscount> CheckoutDiscounts { get; set; }

        public DbSet<FarmHouseCancellationPolicy> FarmHouseCancellationPolicies { get; set; }

        public DbSet<BookingActionToken> BookingActionTokens { get; set; }

        public DbSet<NotificationOutbox> NotificationOutbox { get; set; }

        public DbSet<FarmHousePaymentConfiguration> FarmHousePaymentConfigurations { get; set; }




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

            modelBuilder.Entity<Module>().HasData(
                new Module
                {
                    ModuleId = 1,
                    ModuleName = "Dashboard",
                    DisplayName = "Dashboard",
                    Route = "/dashboard",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Module
                {
                    ModuleId = 2,
                    ModuleName = "Bookings",
                    DisplayName = "Bookings",
                    Route = "/bookings",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Module
                {
                    ModuleId = 3,
                    ModuleName = "Accounts",
                    DisplayName = "Accounts / Finance",
                    Route = "/accounts",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new Module
                {
                    ModuleId = 4,
                    ModuleName = "Reporting",
                    DisplayName = "Reporting",
                    Route = "/reporting",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new Module
                {
                    ModuleId = 5,
                    ModuleName = "WebsiteSettings",
                    DisplayName = "Website Settings",
                    Route = "/website-settings",
                    DisplayOrder = 5,
                    IsActive = true
                }
            );

            #endregion


            #region FarmHouseModule

            modelBuilder.Entity<FarmHouseModule>()
                .HasOne(fhm => fhm.FarmHouse)
                .WithMany(fh => fh.FarmHouseModules)
                .HasForeignKey(fhm => fhm.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmHouseModule>()
                .HasOne(fhm => fhm.Module)
                .WithMany(m => m.FarmHouseModules)
                .HasForeignKey(fhm => fhm.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmHouseModule>()
                .HasIndex(fhm => new
                {
                    fhm.FarmHouseId,
                    fhm.ModuleId
                })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            #endregion


            #region Permission

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.PermissionName)
                .IsUnique();

            modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                PermissionId = 1,
                PermissionName = "Gallery.View",
                Description = "View gallery items",
                DisplayOrder = 1,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 2,
                PermissionName = "Gallery.Create",
                Description = "Create gallery items",
                DisplayOrder = 2,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 3,
                PermissionName = "Gallery.Edit",
                Description = "Edit gallery items",
                DisplayOrder = 3,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 4,
                PermissionName = "Gallery.Delete",
                Description = "Delete gallery items",
                DisplayOrder = 4,
                IsSystemPermission = true,
                IsActive = true
            },

            new Permission
            {
                PermissionId = 5,
                PermissionName = "Amenity.View",
                Description = "View amenities",
                DisplayOrder = 5,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 6,
                PermissionName = "Amenity.Create",
                Description = "Create amenities",
                DisplayOrder = 6,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 7,
                PermissionName = "Amenity.Edit",
                Description = "Edit amenities",
                DisplayOrder = 7,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 8,
                PermissionName = "Amenity.Delete",
                Description = "Delete amenities",
                DisplayOrder = 8,
                IsSystemPermission = true,
                IsActive = true
            },

            new Permission
            {
                PermissionId = 9,
                PermissionName = "Feedback.View",
                Description = "View customer feedback and reviews",
                DisplayOrder = 9,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 10,
                PermissionName = "Feedback.Create",
                Description = "Create customer feedback",
                DisplayOrder = 10,
                IsSystemPermission = true,
                IsActive = true
            },
            new Permission
            {
                PermissionId = 11,
                PermissionName = "Feedback.Delete",
                Description = "Delete customer feedback",
                DisplayOrder = 11,
                IsSystemPermission = true,
                IsActive = true
            }
        );

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



            modelBuilder.ApplyConfiguration(new UserRefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new UserOtpConfiguration());
            modelBuilder.ApplyConfiguration(new GalleryConfiguration());
            modelBuilder.ApplyConfiguration(new FeedbackConfiguration());
            modelBuilder.ApplyConfiguration(new AmenityConfiguration());
            modelBuilder.ApplyConfiguration(new ContactConfiguration());
            modelBuilder.ApplyConfiguration(new AboutUsConfiguration());
            modelBuilder.ApplyConfiguration(new AboutUsFeatureConfiguration());
            modelBuilder.ApplyConfiguration(new BookingConfiguration()); //1
            modelBuilder.ApplyConfiguration(new FarmHousePricingConfiguration());
            modelBuilder.ApplyConfiguration(new BookingEventDateConfiguration());
            modelBuilder.ApplyConfiguration(new BookingDateReservationConfiguration());
            modelBuilder.ApplyConfiguration(new BookingStayDateConfiguration());
            modelBuilder.ApplyConfiguration(new BookingDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new BookingReceiptConfiguration());
            modelBuilder.ApplyConfiguration(new BookingStatusHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new CheckoutDiscountConfiguration());
            modelBuilder.ApplyConfiguration(new FarmHouseCancellationPolicyConfiguration());
            modelBuilder.ApplyConfiguration(new BookingActionTokenConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationOutboxConfiguration());
            modelBuilder.ApplyConfiguration(new FarmHousePaymentConfigurationConfiguration());

        }
    }
}
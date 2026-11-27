using FarmStay.Application.Common.Settings;
using FarmStay.Application.Configurations;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Services.Common;
using FarmStay.Application.Services.Auth;
using FarmStay.Application.Services.Common;
using FarmStay.Infrastructure.BackgroundServices;
using FarmStay.Infrastructure.Data;
using FarmStay.Infrastructure.Repositories.Admin;
using FarmStay.Infrastructure.Repositories.Administration;
using FarmStay.Infrastructure.Repositories.Auth;
using FarmStay.Infrastructure.Repositories.Booking;
using FarmStay.Infrastructure.Repositories.Public;
using FarmStay.Infrastructure.Services.Auth;
using FarmStay.Infrastructure.Services.Common;
using FarmStay.Infrastructure.Services.Communication;
using FarmStay.Infrastructure.Services.WhatsApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FarmStay.API.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // -----------------------------
            // Database
            // -----------------------------

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });

            // -----------------------------
            // Configuration
            // -----------------------------

            services.Configure<EmailSettings>(
                configuration.GetSection("EmailSettings"));

            services.Configure<WhatsAppSettings>(
                configuration.GetSection(WhatsAppSettings.SectionName));

            services.Configure<FileUploadSettings>(
                configuration.GetSection(FileUploadSettings.SectionName));

            // -----------------------------
            // Http Context
            // -----------------------------

            services.AddOptions<RazorpaySettings>()
                .BindConfiguration(RazorpaySettings.SectionName)
                .Validate(settings => !settings.Enabled ||
                    (settings.KeyId.StartsWith("rzp_test_", StringComparison.Ordinal) &&
                     settings.KeyId.Length > "rzp_test_".Length &&
                     !string.IsNullOrWhiteSpace(settings.KeySecret)),
                    "Enabled Razorpay requires a Test Mode key ID and key secret.")
                .ValidateOnStart();

            services.AddHttpContextAccessor();
            services.AddHttpClient(RazorpayAdvancePaymentOrders.ClientName, client =>
            {
                client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(12);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

            // -----------------------------
            // Http Client
            // -----------------------------

            services.AddHttpClient<IWhatsAppService, WhatsAppService>((serviceProvider, client) =>
            {
                var settings = serviceProvider
                    .GetRequiredService<IOptions<WhatsAppSettings>>()
                    .Value;

                client.BaseAddress = new Uri(settings.BaseUrl);
            });

            // -----------------------------
            // Repositories
            // -----------------------------

            services.AddScoped<FarmStay.Infrastructure.Services.Security.CurrentTenantContext>();
            services.AddScoped<FarmStay.Application.Interfaces.Common.ICurrentTenantContext>(sp=>sp.GetRequiredService<FarmStay.Infrastructure.Services.Security.CurrentTenantContext>());
            services.AddScoped<FarmStay.Application.Interfaces.Common.ICurrentUserContext>(sp=>sp.GetRequiredService<FarmStay.Infrastructure.Services.Security.CurrentTenantContext>());
            services.AddScoped<FarmStay.Application.Interfaces.Common.IPublicTenantResolver,FarmStay.Infrastructure.Services.Security.PublicTenantResolver>();
            services.AddScoped<FarmStay.Application.Interfaces.Common.IPublicMediaAccess,FarmStay.Infrastructure.Services.FileUpload.PublicMediaAccess>();
            services.AddScoped<FarmStay.Application.Accounts.IAccountsRepository, FarmStay.Infrastructure.Accounts.AccountsRepository>();
            services.AddScoped<FarmStay.Application.Accounts.IAccountsScope, FarmStay.Infrastructure.Accounts.AccountsScope>();
            services.AddHostedService<FarmStay.Infrastructure.Accounts.AccountingWorker>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IFarmHouseRepository, FarmHouseRepository>();

            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();

            services.AddScoped<IUserMembershipRepository, UserMembershipRepository>();

            services.AddScoped<IPermissionRepository, PermissionRepository>();

            services.AddScoped<IUserOtpRepository, UserOtpRepository>();

            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<IGalleryRepository, GalleryRepository>();

            services.AddScoped<IFeedbackRepository, FeedbackRepository>();

            services.AddScoped<IAmenityRepository, AmenityRepository>();

            services.AddScoped<IAdminMenuRepository, AdminMenuRepository>();

            services.AddScoped<IContactRepository, ContactRepository>();

            services.AddScoped<IAboutUsRepository, AboutUsRepository>();

            services.AddScoped<IPublicPageRepository, PublicPageRepository>();

            services.AddScoped<IFarmHousePricingRepository, FarmHousePricingRepository>();

            services.AddScoped<IBookingCalendarRepository, BookingCalendarRepository>();
            services.AddScoped<IBookingListRepository, BookingListRepository>();
            services.AddScoped<IDashboardRepository, FarmStay.Infrastructure.Repositories.Administration.DashboardRepository>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IBookingDecisionRepository, BookingDecisionService>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IBookingStayRepository, BookingStayOperations>();
            services.AddScoped<IBookingReceipts, FarmStay.Infrastructure.Services.Reporting.BookingReceipts>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IBookingIdentityRepository, BookingIdentityDocuments>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IAdvancePaymentRepository, RazorpayAdvancePaymentOrders>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IAdminBookingConfirmationRepository, AdminBookingConfirmation>();
            services.AddScoped<FarmStay.Application.Reporting.IReportAccess, FarmStay.Infrastructure.Reporting.ReportAccess>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.Reports.IBookingReportRepository,
                FarmStay.Infrastructure.Repositories.Reports.Booking.BookingReportRepository>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.Reports.IRevenueReportRepository,
                FarmStay.Infrastructure.Repositories.Reports.Revenue.RevenueReportRepository>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.Reports.ICustomerReportRepository,
                FarmStay.Infrastructure.Repositories.Reports.Customer.CustomerReportRepository>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.Reports.IOccupancyReportRepository,
                FarmStay.Infrastructure.Repositories.Reports.Occupancy.OccupancyReportRepository>();
            services.AddScoped<FarmStay.Application.Interfaces.Services.Reports.IReportFileRenderer,
                FarmStay.Infrastructure.Reporting.ReportFileRenderer>();
            services.AddScoped<FarmStay.Application.Interfaces.Repositories.IBookingSubmissionRepository, BookingSubmission>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // -----------------------------
            // Infrastructure Services
            // -----------------------------

            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<ITenantResolver, TenantResolver>();

            services.AddScoped<FarmStay.Application.Interfaces.Repositories.FileUpload.IFileUploadRepository, FarmStay.Infrastructure.Services.Common.FileUploadRepository>();

            // -----------------------------
            // Comman Services
            // -----------------------------

            services.AddScoped<IPasswordService, PasswordService>();

            // -----------------------------
            // Background Services
            // -----------------------------

            // Email
            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddHostedService<EmailBackgroundService>();

            // WhatsApp
            services.AddSingleton<IWhatsAppQueue, WhatsAppQueue>();
            services.AddHostedService<WhatsAppBackgroundService>();
            services.AddHostedService<BookingNotificationBackgroundService>();
            services.AddHostedService<BookingOwnerApprovalBackgroundService>();
            services.AddScoped<IBookingNotifications, BookingNotifications>();

            return services;
        }
    }
}

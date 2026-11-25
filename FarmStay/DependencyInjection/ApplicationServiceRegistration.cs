using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Application.Interfaces.Services.Administration;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Application.Services.Admin;
using FarmStay.Application.Services.Administration;
using FarmStay.Application.Services.Auth;
using FarmStay.Application.Services.Booking;
using FarmStay.Application.Services.Public;

namespace FarmStay.API.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Application Services
            services.AddScoped<FarmStay.Application.Accounts.IAccountsService, FarmStay.Application.Accounts.AccountsService>();

            services.AddScoped<IAdminBookingCustomers, FarmStay.Application.Services.Booking.AdminBookingCustomers>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IFarmHouseService, FarmHouseService>();

            services.AddScoped<IPublicSiteService, PublicSiteService>();

            services.AddScoped<IGalleryService, GalleryService>();

            services.AddScoped<IFeedbackService, FeedbackService>();

            services.AddScoped<IAmenityService, AmenityService>();

            services.AddScoped<IPermissionService, PermissionService>();

            services.AddScoped<IAdminMenuService, AdminMenuService>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IContactService, ContactService>();

            services.AddScoped<IAboutUsService, AboutUsService>();

            services.AddScoped<IPublicPageService, PublicPageService>();

            services.AddScoped<IFarmHousePricingService, FarmHousePricingService>();

            services.AddScoped<IBookingCalendarService, BookingCalendarService>();
            services.AddScoped<IBookingListService, BookingListService>();
            services.AddScoped<FarmStay.Application.Interfaces.Services.Reports.IBookingReportService,
                FarmStay.Application.Services.Reports.Booking.BookingReportService>();
            services.AddScoped<FarmStay.Application.Interfaces.Services.Reports.IRevenueReportService,
                FarmStay.Application.Services.Reports.Revenue.RevenueReportService>();
            services.AddScoped<FarmStay.Application.Interfaces.Services.Reports.ICustomerReportService,
                FarmStay.Application.Services.Reports.Customer.CustomerReportService>();
            services.AddScoped<FarmStay.Application.Interfaces.Services.Reports.IOccupancyReportService,
                FarmStay.Application.Services.Reports.Occupancy.OccupancyReportService>();
            services.AddScoped<FarmStay.Application.Reporting.IReportingService, FarmStay.Application.Services.Reports.ReportingService>();



            return services;
        }
    }
}

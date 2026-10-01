using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Application.Interfaces.Services.Administration;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Application.Services.Admin;
using FarmStay.Application.Services.Administration;
using FarmStay.Application.Services.Auth;
using FarmStay.Application.Services.Public;

namespace FarmStay.API.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Application Services

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IFarmHouseService, FarmHouseService>();

            services.AddScoped<IPublicSiteService, PublicSiteService>();

            services.AddScoped<IGalleryService, GalleryService>();

            services.AddScoped<IFeedbackService, FeedbackService>();

            services.AddScoped<IAmenityService, AmenityService>();

            services.AddScoped<IPermissionService, PermissionService>();

            services.AddScoped<IAdminMenuService, AdminMenuService>();

            services.AddScoped<IContactService, ContactService>();

            services.AddScoped<IAboutUsService, AboutUsService>();

            services.AddScoped<IPublicPageService, PublicPageService>();


            return services;
        }
    }
}
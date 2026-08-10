using FarmStay.Application.Interfaces.Services.Admin;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Application.Services.Admin;
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

            

            return services;
        }
    }
}
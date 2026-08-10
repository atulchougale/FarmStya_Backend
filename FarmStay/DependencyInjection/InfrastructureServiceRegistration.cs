using FarmStay.Application.Common.Settings;
using FarmStay.Application.Configurations;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Services.Auth;
using FarmStay.Application.Services.Common;
using FarmStay.Infrastructure.BackgroundServices;
using FarmStay.Infrastructure.Data;
using FarmStay.Infrastructure.Repositories.Admin;
using FarmStay.Infrastructure.Repositories.Auth;
using FarmStay.Infrastructure.Services.Auth;
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

            // -----------------------------
            // Http Context
            // -----------------------------

            services.AddHttpContextAccessor();

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

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IFarmHouseRepository, FarmHouseRepository>();

            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();

            services.AddScoped<IUserMembershipRepository, UserMembershipRepository>();

            services.AddScoped<IUserOtpRepository, UserOtpRepository>();

            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // -----------------------------
            // Infrastructure Services
            // -----------------------------

            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<ITenantResolver, TenantResolver>();

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

            return services;
        }
    }
}
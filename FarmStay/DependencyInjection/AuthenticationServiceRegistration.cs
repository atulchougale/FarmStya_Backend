using FarmStay.API.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FarmStay.API.DependencyInjection
{
    public static class AuthenticationServiceRegistration
    {
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // -----------------------------
            // Authentication
            // -----------------------------

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer =
                                configuration["Jwt:Issuer"],

                            ValidAudience =
                                configuration["Jwt:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        configuration["Jwt:Key"]!))
                        };
                });

            // -----------------------------
            // Authorization
            // -----------------------------

            services.AddAuthorization();

            services.AddSingleton<
                IAuthorizationPolicyProvider,
                PermissionPolicyProvider>();

            services.AddScoped<
                IAuthorizationHandler,
                PermissionAuthorizationHandler>();

            return services;
        }
    }
}
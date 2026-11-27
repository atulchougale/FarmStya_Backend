using Microsoft.Extensions.DependencyInjection;

namespace FarmStay.API.DependencyInjection
{
    public static class CorsServiceRegistration
    {
        public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var origins=configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? (environment.IsDevelopment()?new[]{"http://localhost:4200","http://127.0.0.1:4200"}:Array.Empty<string>());
            if(origins.Length==0||origins.Any(x=>!Uri.TryCreate(x,UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https")||x.Contains('*'))) throw new InvalidOperationException("Configure explicit Cors:AllowedOrigins.");
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}

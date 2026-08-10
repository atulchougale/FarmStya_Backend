using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FarmStay.API.DependencyInjection
{
    public static class SerilogServiceRegistration
    {
        public static ConfigureHostBuilder AddSerilogServices(this ConfigureHostBuilder host)
        {
            host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            return host;
        }
    }
}
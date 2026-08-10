using FarmStay.Application.Validators.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FarmStay.API.DependencyInjection
{
    public static class FluentValidationServiceRegistration
    {
        public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
        {
            services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

            services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();

            return services;
        }
    }
}
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photobiz.Application.Common.Behaviors;
using Photobiz.Application.Common.Settings;

namespace Photobiz.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>();
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

            TypeAdapterConfig.GlobalSettings.Scan(typeof(ApplicationAssemblyMarker).Assembly);
            services.AddMapster();

            return services;
        }
    }
}

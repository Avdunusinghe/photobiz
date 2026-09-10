using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photobiz.Application.Common.Behaviors;
using Photobiz.Application.Common.Settings;
using Photobiz.Domain.Entities;

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

            var mapsterConfig = TypeAdapterConfig.GlobalSettings;
            mapsterConfig.Scan(typeof(ApplicationAssemblyMarker).Assembly);
            services.AddSingleton(mapsterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Interceptors;

namespace Photobiz.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

            services.AddDbContext<PhotobizDbContext>((serviceProvider, options) => options
                .UseSqlServer(connectionString)
                .UseLazyLoadingProxies()
                .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>())
                // The User soft-delete query filter is safe here: UserRole / Gallery are only ever
                // loaded through a (live) User, never queried as roots joined to a deleted principal.
                .ConfigureWarnings(warnings => warnings.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PhotobizDbContext>());

            return services;
        }
    }
}

using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Interceptors;
using Photobiz.Infrastructure.Tenancy;

namespace Photobiz.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Used only to construct the DbContext instance; TenantSelectionMiddleware overwrites
            // it with the resolved tenant's connection string before any handler runs. Pointing it
            // at a real (dev) database rather than an empty string keeps design-time tooling
            // (`dotnet ef migrations add`) and any code path that runs outside a request (seeding,
            // background jobs) working against a sensible default.
            var tenantConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            var masterConnectionString = configuration.GetConnectionString("MasterConnection")
                ?? throw new InvalidOperationException("Connection string 'MasterConnection' is not configured.");

            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

            services.AddDbContext<MasterDbContext>((serviceProvider, options) => options
                .UseSqlServer(masterConnectionString)
                .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>()));

            services.AddDbContext<PhotobizDbContext>((serviceProvider, options) => options
                .UseSqlServer(tenantConnectionString)
                .UseLazyLoadingProxies()
                .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>())
                // The User soft-delete query filter is safe here: UserRole / Gallery are only ever
                // loaded through a (live) User, never queried as roots joined to a deleted principal.
                .ConfigureWarnings(warnings => warnings.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PhotobizDbContext>());

            services.AddSingleton<ILookupClient>(new LookupClient());
            services.AddSingleton<ITxtRecordLookup, DnsClientTxtRecordLookup>();
            services.AddSingleton<ICustomDomainVerifier, DnsTxtCustomDomainVerifier>();

            return services;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure.Persistence;

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

            services.AddDbContext<PhotobizDbContext>(options => options
                .UseSqlServer(connectionString)
                .UseLazyLoadingProxies());

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PhotobizDbContext>());

            return services;
        }
    }
}

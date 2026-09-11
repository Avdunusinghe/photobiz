using Microsoft.AspNetCore.Cors.Infrastructure;
using Photobiz.Api.Cors;

namespace Photobiz.Api.Extensions
{
    public static class CorsExtensions
    {
        public const string PolicyName = "Default";

        public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
        {
            services.AddCors();

            // Overrides AddCors()'s DefaultCorsPolicyProvider: the admin SPA's origin is a fixed
            // config value, but a tenant portfolio's origin is only known via the Master database
            // at request time (see TenantCorsPolicyProvider).
            services.AddSingleton<ICorsPolicyProvider, TenantCorsPolicyProvider>();

            return services;
        }
    }
}

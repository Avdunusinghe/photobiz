using Microsoft.AspNetCore.Cors.Infrastructure;
using Photobiz.Api.Extensions;
using Photobiz.Application.Common.Interfaces;

namespace Photobiz.Api.Cors
{
    /// <summary>
    /// Builds the CORS policy per request instead of from a fixed list, because the admin SPA's
    /// origin is static config but a tenant's portfolio site's origin is whatever domain/subdomain
    /// is registered for them in the Master database — unbounded and only known at runtime.
    /// Replaces the default <see cref="ICorsPolicyProvider"/> registered by <c>AddCors()</c>.
    /// </summary>
    public sealed class TenantCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly string[] _staticAllowedOrigins;

        public TenantCorsPolicyProvider(IConfiguration configuration)
        {
            _staticAllowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        }

        public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
        {
            if (!string.Equals(policyName, CorsExtensions.PolicyName, StringComparison.Ordinal))
            {
                return null;
            }

            var builder = new CorsPolicyBuilder().AllowAnyHeader().AllowAnyMethod();

            var origin = context.Request.Headers.Origin.ToString();

            if (string.IsNullOrEmpty(origin))
            {
                // No Origin header: not a cross-origin request, so CORS doesn't come into play —
                // the specific origin list on the returned policy is moot either way.
                return builder.WithOrigins(_staticAllowedOrigins).Build();
            }

            if (_staticAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                return builder.WithOrigins(origin).Build();
            }

            if (Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            {
                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                var connectionString = await tenantService.GetTenantConnectionStringByHostAsync(
                    originUri.Host, context.RequestAborted);

                if (!string.IsNullOrEmpty(connectionString))
                {
                    return builder.WithOrigins(origin).Build();
                }
            }

            // Unknown origin: a policy with no allowed origins, not null — null would fall back to
            // whatever ambient default policy exists, which could be more permissive than intended.
            return builder.Build();
        }
    }
}

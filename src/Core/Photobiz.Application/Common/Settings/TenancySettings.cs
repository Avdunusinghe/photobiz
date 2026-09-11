namespace Photobiz.Application.Common.Settings
{
    /// <summary>
    /// Configures the platform's own domain so a request's Host header can be split into
    /// "tenant subdomain" + "our base domain" for public, unauthenticated traffic (portfolio
    /// sites) — as opposed to a verified <c>Tenant.CustomDomain</c>, which is matched exactly.
    /// </summary>
    public class TenancySettings
    {
        /// <summary>e.g. "photobiz.app", so "janedoe.photobiz.app" resolves to tenant key "janedoe".</summary>
        public required string BaseDomain { get; set; }
    }
}

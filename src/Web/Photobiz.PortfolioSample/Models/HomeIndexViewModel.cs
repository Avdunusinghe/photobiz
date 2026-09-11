using Photobiz.PortfolioSample.Services;

namespace Photobiz.PortfolioSample.Models
{
    public class HomeIndexViewModel
    {
        // Matches TenantSeeder.DefaultTenantKey and appsettings' "Tenancy:BaseDomain" so the
        // out-of-the-box default actually resolves against a freshly-seeded dev database.
        public string SimulatedHost { get; set; } = "default.photobiz.app";

        public ApiCallResult? TenantCheckResult { get; set; }

        public string TenantKey { get; set; } = "default";

        // Matches UserSeeder's default admin credentials.
        public string Username { get; set; } = "admin";

        public string Password { get; set; } = "Photobiz!2026";

        public ApiCallResult? LoginResult { get; set; }
    }
}

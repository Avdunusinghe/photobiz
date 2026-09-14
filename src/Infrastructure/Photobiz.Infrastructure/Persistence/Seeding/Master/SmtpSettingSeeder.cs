using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities.Master;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Persistence.Seeding.Master
{
    /// <summary>
    /// Gives the local dev tenant a sample SMTP configuration on first run, so the "Email (SMTP)"
    /// view on the Business Profile screen has something to show without a manual DB insert.
    /// Points at a sandbox host (Mailtrap) rather than a real mail server — this is sample data for
    /// local development, not a working credential.
    /// </summary>
    public static class SmtpSettingSeeder
    {
        public static async Task SeedAsync(
            MasterDbContext masterDbContext,
            string tenantKey,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var tenant = await masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken);

            if (tenant is null)
            {
                return;
            }

            var exists = await masterDbContext.SmtpSettings
                .AnyAsync(s => s.TenantId == tenant.Id, cancellationToken);

            if (exists)
            {
                return;
            }

            var smtpSetting = SmtpSetting.Create(
                tenantId: tenant.Id,
                host: "sandbox.smtp.mailtrap.io",
                port: 587,
                username: "dev-sample-username",
                password: "dev-sample-password",
                enableSsl: true,
                fromEmail: "no-reply@photobiz.local",
                fromName: tenant.Name);

            masterDbContext.SmtpSettings.Add(smtpSetting);
            await masterDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded sample SMTP settings for tenant '{TenantKey}'.", tenantKey);
        }
    }
}

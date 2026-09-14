using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities.Master
{
    /// <summary>
    /// A tenant's own outgoing-mail configuration (so their emails — booking confirmations,
    /// invoices, etc. — arrive "from" their own business rather than the platform's shared
    /// sender). Rows live in the <b>Master</b> database, one per tenant at most; a tenant with no
    /// row here simply hasn't configured this yet and falls back to the platform's default sender.
    ///
    /// <see cref="Password"/> is the SMTP account's credential — treat it exactly like
    /// <c>Tenant.CardNo</c>/<c>CardExpiry</c>: this column should be encrypted at rest once the
    /// platform has an encryption-at-rest story, not stored as plain text long-term.
    /// </summary>
    public class SmtpSetting : AuditableEntity<Guid>
    {
        public Guid TenantId { get; private set; }

        public virtual Tenant Tenant { get; private set; } = null!;

        public string Host { get; private set; } = default!;

        public int Port { get; private set; }

        public string Username { get; private set; } = default!;

        public string Password { get; private set; } = default!;

        public bool EnableSsl { get; private set; }

        public string FromEmail { get; private set; } = default!;

        public string? FromName { get; private set; }

        /// <summary>Lets a tenant temporarily fall back to the platform's default sender without losing their saved configuration.</summary>
        public bool IsEnabled { get; private set; }

        /// <summary>Required by EF Core for materialization; use <see cref="Create"/> otherwise.</summary>
        private SmtpSetting()
        {
        }

        public static SmtpSetting Create(
            Guid tenantId,
            string host,
            int port,
            string username,
            string password,
            bool enableSsl,
            string fromEmail,
            string? fromName)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Tenant id is required.", nameof(tenantId));
            }

            var setting = new SmtpSetting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                IsEnabled = true
            };

            setting.UpdateConnection(host, port, username, password, enableSsl);
            setting.UpdateSender(fromEmail, fromName);

            return setting;
        }

        /// <summary>Replaces the SMTP server/credential details.</summary>
        public void UpdateConnection(string host, int port, string username, string password, bool enableSsl)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("SMTP host is required.", nameof(host));
            }

            if (port is <= 0 or > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), port, "SMTP port must be between 1 and 65535.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("SMTP username is required.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("SMTP password is required.", nameof(password));
            }

            Host = host.Trim();
            Port = port;
            Username = username.Trim();
            Password = password;
            EnableSsl = enableSsl;
        }

        /// <summary>Replaces the "From" address tenant emails are sent as.</summary>
        public void UpdateSender(string fromEmail, string? fromName)
        {
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new ArgumentException("From email is required.", nameof(fromEmail));
            }

            FromEmail = fromEmail.Trim();
            FromName = string.IsNullOrWhiteSpace(fromName) ? null : fromName.Trim();
        }

        public void Enable() => IsEnabled = true;

        /// <summary>Falls back to the platform's default sender without discarding the saved configuration.</summary>
        public void Disable() => IsEnabled = false;
    }
}

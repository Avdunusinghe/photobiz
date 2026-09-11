using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities
{
    /// <summary>
    /// A registered tenant/workspace: which physical database its data lives in, the customer who
    /// owns it, and its subscription/billing state (PayHere is the payment gateway this shape was
    /// modeled on). Rows live in the <b>Master</b> database only, resolved by
    /// <c>TenantSelectionMiddleware</c>/<c>ITenantService</c> to route each request to the right
    /// tenant database — never queried from inside a tenant database itself.
    ///
    /// Unlike the plain settable entities elsewhere in the domain, this is a small rich model:
    /// construction goes through <see cref="Create"/> and state changes through named methods, so
    /// invariants (a non-blank key, a coherent subscription state) can't be bypassed by assigning
    /// properties directly. The handful of raw PayHere passthrough fields are left publicly
    /// settable since they're just a webhook payload snapshot, not state the domain reasons about.
    /// <see cref="CardNo"/> / <see cref="CardExpiry"/> are expected to hold the masked/tokenized
    /// values PayHere returns, never a raw PAN.
    /// </summary>
    public class Tenant : AuditableEntity<Guid>
    {
        // ── Core ─────────────────────────────────────────────────────────────────
        public string TenantKey { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public string ConnectionString { get; private set; } = default!;

        /// <summary>
        /// The tenant's own domain for their public portfolio site (e.g. "www.janedoephoto.com"),
        /// once ownership has been proven. Null until <see cref="MarkCustomDomainVerified"/> runs —
        /// <c>ITenantService</c> must never route traffic to an unverified domain, so a lapsed
        /// verification never silently serves the wrong tenant's data to whoever controls that
        /// domain next. The subdomain built from <see cref="TenantKey"/> always keeps working
        /// regardless of custom domain state.
        /// </summary>
        public string? CustomDomain { get; private set; }

        public DateTime? CustomDomainVerifiedAt { get; private set; }

        /// <summary>
        /// The value the tenant must publish as a DNS TXT record (see <c>ICustomDomainVerifier</c>)
        /// to prove they control <see cref="CustomDomain"/> before it goes live.
        /// </summary>
        public string? DomainVerificationToken { get; private set; }

        // ── Customer profile (stored for future reference) ────────────────────────
        public string CustomerEmail { get; private set; } = default!;
        public string CustomerFirstName { get; private set; } = default!;
        public string CustomerLastName { get; private set; } = default!;
        public string Phone { get; private set; } = default!;
        public string Address { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public string Country { get; private set; } = default!;

        // ── Subscription ─────────────────────────────────────────────────────────
        public string PlanCode { get; private set; } = default!;
        public string BillingCycle { get; private set; } = default!;
        public Guid OrderId { get; private set; }
        public string? PayHerePaymentId { get; private set; }
        public string? SubscriptionId { get; private set; }
        public string? PayhereAmount { get; set; }
        public string? PayhereCurrency { get; set; }
        public string? Method { get; set; }
        public string? Recurring { get; set; }
        public string? ItemRecurrence { get; set; }
        public string? ItemDuration { get; set; }
        public string? ItemRecStatus { get; set; }
        public string? ItemRecDateNext { get; set; }
        public string? ItemRecInstallPaid { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardNo { get; set; }
        public string? CardExpiry { get; set; }

        public bool IsSubscribed { get; private set; }
        public DateOnly? SubscriptionExpiredOn { get; private set; }

        /// <summary>Required by EF Core for materialization; use <see cref="Create"/> otherwise.</summary>
        private Tenant()
        {
        }

        /// <summary>
        /// Provisions a new tenant registration. Access is granted immediately (a fresh signup
        /// shouldn't have to wait on a payment webhook to log in); a failed/lapsed renewal later
        /// calls <see cref="ExpireSubscription"/>.
        /// </summary>
        public static Tenant Create(
            string tenantKey,
            string name,
            string connectionString,
            string customerEmail,
            string customerFirstName,
            string customerLastName,
            string phone,
            string address,
            string city,
            string country,
            string planCode,
            string billingCycle,
            Guid orderId)
        {
            if (string.IsNullOrWhiteSpace(tenantKey))
            {
                throw new ArgumentException("Tenant key is required.", nameof(tenantKey));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            return new Tenant
            {
                Id = Guid.NewGuid(),
                TenantKey = tenantKey.Trim(),
                Name = name.Trim(),
                ConnectionString = connectionString,
                CustomerEmail = customerEmail.Trim(),
                CustomerFirstName = customerFirstName.Trim(),
                CustomerLastName = customerLastName.Trim(),
                Phone = phone.Trim(),
                Address = address.Trim(),
                City = city.Trim(),
                Country = country.Trim(),
                PlanCode = planCode.Trim(),
                BillingCycle = billingCycle.Trim(),
                OrderId = orderId,
                IsSubscribed = true
            };
        }

        /// <summary>Repoints the tenant at a different physical database, e.g. after a migration to a new server.</summary>
        public void UpdateConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            ConnectionString = connectionString;
        }

        /// <summary>
        /// Starts (or restarts) verification of a custom domain: stores it as pending and issues a
        /// fresh verification token. Does not touch a previously *verified* domain, which stays
        /// live until the new one actually verifies — a tenant mid-switch never loses their site.
        /// </summary>
        public void RequestCustomDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("Domain is required.", nameof(domain));
            }

            var normalized = domain.Trim().ToLowerInvariant();

            if (string.Equals(normalized, CustomDomain, StringComparison.Ordinal))
            {
                return;
            }

            CustomDomain = normalized;
            CustomDomainVerifiedAt = null;
            DomainVerificationToken = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Marks <see cref="CustomDomain"/> live. Call only after <c>ICustomDomainVerifier</c> has
        /// confirmed the tenant actually controls the domain's DNS — this method itself performs no
        /// verification, it just records that verification already happened.
        /// </summary>
        public void MarkCustomDomainVerified()
        {
            if (CustomDomain is null)
            {
                throw new InvalidOperationException("No custom domain is pending verification.");
            }

            CustomDomainVerifiedAt = DateTime.UtcNow;
            DomainVerificationToken = null;
        }

        /// <summary>Detaches the custom domain; the tenant's subdomain keeps working unaffected.</summary>
        public void RemoveCustomDomain()
        {
            CustomDomain = null;
            CustomDomainVerifiedAt = null;
            DomainVerificationToken = null;
        }

        /// <summary>Records a successful PayHere payment/recurring notification and (re)grants access.</summary>
        public void RecordSubscriptionPayment(string payHerePaymentId, string? subscriptionId, DateOnly expiresOn)
        {
            if (string.IsNullOrWhiteSpace(payHerePaymentId))
            {
                throw new ArgumentException("PayHere payment id is required.", nameof(payHerePaymentId));
            }

            PayHerePaymentId = payHerePaymentId;
            SubscriptionId = subscriptionId;
            SubscriptionExpiredOn = expiresOn;
            IsSubscribed = true;
        }

        /// <summary>Revokes access after a lapsed, cancelled, or failed subscription payment.</summary>
        public void ExpireSubscription() => IsSubscribed = false;
    }
}

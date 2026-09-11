using Photobiz.Application.Common.Interfaces;

namespace Photobiz.Infrastructure.Tenancy
{
    /// <summary>
    /// Verifies custom-domain ownership by checking for a DNS TXT record at
    /// "_photobiz-verify.{domain}" containing the token <c>Tenant.RequestCustomDomain</c> issued —
    /// the same ownership-proof pattern used by Google Search Console, Stripe custom domains, etc.
    /// </summary>
    public sealed class DnsTxtCustomDomainVerifier : ICustomDomainVerifier
    {
        private const string VerificationSubdomainPrefix = "_photobiz-verify";

        private readonly ITxtRecordLookup _txtRecordLookup;

        public DnsTxtCustomDomainVerifier(ITxtRecordLookup txtRecordLookup)
        {
            _txtRecordLookup = txtRecordLookup;
        }

        public async Task<bool> IsOwnershipVerifiedAsync(
            string domain,
            string verificationToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(verificationToken))
            {
                return false;
            }

            var records = await _txtRecordLookup.LookupAsync(
                $"{VerificationSubdomainPrefix}.{domain}", cancellationToken);

            return records.Any(text => string.Equals(text.Trim(), verificationToken, StringComparison.Ordinal));
        }
    }
}

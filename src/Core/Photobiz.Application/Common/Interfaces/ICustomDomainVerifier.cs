namespace Photobiz.Application.Common.Interfaces
{
    /// <summary>
    /// Proves a tenant actually controls a custom domain before it's trusted for routing. The
    /// standard approach: the tenant publishes a DNS TXT record containing the token issued by
    /// <c>Tenant.RequestCustomDomain</c>; this checks that record is present.
    /// </summary>
    public interface ICustomDomainVerifier
    {
        Task<bool> IsOwnershipVerifiedAsync(
            string domain,
            string verificationToken,
            CancellationToken cancellationToken = default);
    }
}

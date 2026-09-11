namespace Photobiz.Application.Common.Constants
{
    /// <summary>
    /// Custom JWT claim types used for tenant resolution. Shared between token issuance
    /// (<c>IssueTokenCommandHandler</c>) and the web layer's tenant lookup (<c>ITenantService</c>)
    /// so both sides agree on the claim name.
    /// </summary>
    public static class TenantClaimTypes
    {
        public const string TenantKey = "tenant_key";
    }
}

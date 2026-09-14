namespace Photobiz.Application.Features.Tenants.Common
{
    /// <summary>
    /// The current tenant's own profile, as shown/edited on their "tenant settings" screen.
    /// Deliberately excludes <c>ConnectionString</c> and the raw PayHere payment fields on
    /// <c>Tenant</c> — nothing here is sensitive enough to withhold from the tenant themselves, but
    /// those two groups are either infrastructure detail or third-party payload snapshot, not
    /// profile data.
    /// </summary>
    public record TenantDetailsDto(
        Guid Id,
        string TenantKey,
        string Name,
        string? LogoUrl,
        string CustomerEmail,
        string CustomerFirstName,
        string CustomerLastName,
        string Phone,
        string Address,
        string City,
        string Country,
        string PlanCode,
        string BillingCycle,
        bool IsSubscribed,
        DateOnly? SubscriptionExpiredOn,
        string? CustomDomain,
        DateTime? CustomDomainVerifiedAt,
        DateTime CreatedAt);
}

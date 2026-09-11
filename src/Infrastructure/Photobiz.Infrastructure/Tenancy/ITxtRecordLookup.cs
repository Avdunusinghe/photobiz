namespace Photobiz.Infrastructure.Tenancy
{
    /// <summary>
    /// Thin seam over the actual DNS client so <see cref="DnsTxtCustomDomainVerifier"/>'s matching
    /// logic can be unit tested without depending on DnsClient's own (hard to construct) response
    /// types.
    /// </summary>
    public interface ITxtRecordLookup
    {
        Task<IReadOnlyList<string>> LookupAsync(string queryName, CancellationToken cancellationToken = default);
    }
}

using DnsClient;

namespace Photobiz.Infrastructure.Tenancy
{
    /// <summary>Real DNS TXT lookup, backed by the DnsClient package.</summary>
    public sealed class DnsClientTxtRecordLookup : ITxtRecordLookup
    {
        private readonly ILookupClient _lookupClient;

        public DnsClientTxtRecordLookup(ILookupClient lookupClient)
        {
            _lookupClient = lookupClient;
        }

        public async Task<IReadOnlyList<string>> LookupAsync(
            string queryName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _lookupClient.QueryAsync(queryName, QueryType.TXT, cancellationToken: cancellationToken);

                return result.Answers.TxtRecords()
                    .SelectMany(record => record.Text)
                    .ToList();
            }
            catch (DnsResponseException)
            {
                // NXDOMAIN, no TXT record yet, resolver timeout, ... — all read the same as
                // "nothing published there yet" rather than an application error.
                return [];
            }
        }
    }
}

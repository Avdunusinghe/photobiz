using Photobiz.Infrastructure.Tenancy;

namespace Photobiz.Infrastructure.Tests.Tenancy
{
    public class DnsTxtCustomDomainVerifierTests
    {
        private sealed class FakeTxtRecordLookup : ITxtRecordLookup
        {
            private readonly Dictionary<string, IReadOnlyList<string>> _records = new(StringComparer.OrdinalIgnoreCase);

            public string? RequestedQueryName { get; private set; }

            public void SetRecords(string queryName, params string[] values) => _records[queryName] = values;

            public Task<IReadOnlyList<string>> LookupAsync(string queryName, CancellationToken cancellationToken = default)
            {
                RequestedQueryName = queryName;
                return Task.FromResult(_records.TryGetValue(queryName, out var values) ? values : []);
            }
        }

        [Fact]
        public async Task IsOwnershipVerifiedAsync_QueriesTheVerificationSubdomain()
        {
            var lookup = new FakeTxtRecordLookup();
            lookup.SetRecords("_photobiz-verify.acmestudio.test", "token-123");
            var verifier = new DnsTxtCustomDomainVerifier(lookup);

            await verifier.IsOwnershipVerifiedAsync("acmestudio.test", "token-123");

            Assert.Equal("_photobiz-verify.acmestudio.test", lookup.RequestedQueryName);
        }

        [Fact]
        public async Task IsOwnershipVerifiedAsync_WithMatchingTxtRecord_ReturnsTrue()
        {
            var lookup = new FakeTxtRecordLookup();
            lookup.SetRecords("_photobiz-verify.acmestudio.test", "some-other-value", "token-123");
            var verifier = new DnsTxtCustomDomainVerifier(lookup);

            Assert.True(await verifier.IsOwnershipVerifiedAsync("acmestudio.test", "token-123"));
        }

        [Fact]
        public async Task IsOwnershipVerifiedAsync_TrimsWhitespaceAroundTheRecordValue()
        {
            var lookup = new FakeTxtRecordLookup();
            lookup.SetRecords("_photobiz-verify.acmestudio.test", "  token-123  ");
            var verifier = new DnsTxtCustomDomainVerifier(lookup);

            Assert.True(await verifier.IsOwnershipVerifiedAsync("acmestudio.test", "token-123"));
        }

        [Fact]
        public async Task IsOwnershipVerifiedAsync_WithNoMatchingRecord_ReturnsFalse()
        {
            var lookup = new FakeTxtRecordLookup();
            lookup.SetRecords("_photobiz-verify.acmestudio.test", "wrong-token");
            var verifier = new DnsTxtCustomDomainVerifier(lookup);

            Assert.False(await verifier.IsOwnershipVerifiedAsync("acmestudio.test", "token-123"));
        }

        [Fact]
        public async Task IsOwnershipVerifiedAsync_WithNoRecordAtAll_ReturnsFalse()
        {
            var verifier = new DnsTxtCustomDomainVerifier(new FakeTxtRecordLookup());

            Assert.False(await verifier.IsOwnershipVerifiedAsync("acmestudio.test", "token-123"));
        }

        [Theory]
        [InlineData("", "token-123")]
        [InlineData("acmestudio.test", "")]
        [InlineData(null, "token-123")]
        public async Task IsOwnershipVerifiedAsync_WithBlankInput_ReturnsFalseWithoutQuerying(
            string? domain, string verificationToken)
        {
            var lookup = new FakeTxtRecordLookup();
            var verifier = new DnsTxtCustomDomainVerifier(lookup);

            Assert.False(await verifier.IsOwnershipVerifiedAsync(domain!, verificationToken));
            Assert.Null(lookup.RequestedQueryName);
        }
    }
}

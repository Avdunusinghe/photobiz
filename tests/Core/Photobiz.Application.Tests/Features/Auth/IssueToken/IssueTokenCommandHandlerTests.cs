using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Photobiz.Application.Common.Settings;
using Photobiz.Application.Features.Auth.IssueToken;

namespace Photobiz.Application.Tests.Features.Auth.IssueToken
{
    public class IssueTokenCommandHandlerTests
    {
        private static readonly JwtSettings Settings = new()
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "unit-test-signing-key-that-is-long-enough-for-hmacsha256",
            ExpiryMinutes = 30
        };

        private readonly IssueTokenCommandHandler _handler = new(Options.Create(Settings));

        [Fact]
        public async Task Handle_ReturnsTokenWithExpectedClaimsAndExpiry()
        {
            var before = DateTime.UtcNow;

            var result = await _handler.Handle(new IssueTokenCommand("someone"), CancellationToken.None);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

            Assert.Equal(Settings.Issuer, token.Issuer);
            Assert.Equal(Settings.Audience, token.Audiences.Single());
            Assert.Equal("someone", token.Subject);
            Assert.InRange(result.ExpiresAtUtc, before.AddMinutes(Settings.ExpiryMinutes), before.AddMinutes(Settings.ExpiryMinutes).AddSeconds(5));
        }

        [Fact]
        public async Task Handle_CalledTwice_ProducesDifferentTokenIds()
        {
            var first = await _handler.Handle(new IssueTokenCommand("someone"), CancellationToken.None);
            var second = await _handler.Handle(new IssueTokenCommand("someone"), CancellationToken.None);

            var handler = new JwtSecurityTokenHandler();
            var firstJti = handler.ReadJwtToken(first.AccessToken).Id;
            var secondJti = handler.ReadJwtToken(second.AccessToken).Id;

            Assert.NotEqual(firstJti, secondJti);
        }
    }
}

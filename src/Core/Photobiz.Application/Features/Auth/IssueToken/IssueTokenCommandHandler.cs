using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Photobiz.Application.Common.Settings;

namespace Photobiz.Application.Features.Auth.IssueToken
{
    public class IssueTokenCommandHandler : IRequestHandler<IssueTokenCommand, IssueTokenResult>
    {
        private readonly JwtSettings _jwtSettings;

        public IssueTokenCommandHandler(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public Task<IssueTokenResult> Handle(IssueTokenCommand request, CancellationToken cancellationToken)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                expires: expires,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(new IssueTokenResult(accessToken, expires));
        }
    }
}

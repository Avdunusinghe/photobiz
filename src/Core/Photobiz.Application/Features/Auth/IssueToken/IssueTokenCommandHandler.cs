using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Settings;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Auth.IssueToken
{
    public class IssueTokenCommandHandler : IRequestHandler<IssueTokenCommand, IssueTokenResult>
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public IssueTokenCommandHandler(
            IOptions<JwtSettings> jwtSettings,
            IApplicationDbContext dbContext,
            IPasswordHasher<User> passwordHasher)
        {
            _jwtSettings = jwtSettings.Value;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<IssueTokenResult> Handle(IssueTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

            if (user is null ||
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                throw new AuthenticationFailedException();
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));

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

            return new IssueTokenResult(accessToken, expires);
        }
    }
}

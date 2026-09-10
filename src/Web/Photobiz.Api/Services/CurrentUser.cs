using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Photobiz.Application.Common.Interfaces;

namespace Photobiz.Api.Services
{
    /// <summary>
    /// Resolves the caller from the authenticated principal on the current HTTP request.
    /// Returns <c>null</c> when there is no request or the request is anonymous.
    /// </summary>
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserName
        {
            get
            {
                var principal = _httpContextAccessor.HttpContext?.User;

                if (principal?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                // The token issued by IssueTokenCommandHandler carries the username in "sub";
                // fall back to the mapped claim types in case inbound claim mapping is enabled.
                return principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue(ClaimTypes.Name)
                    ?? principal.Identity.Name;
            }
        }
    }
}

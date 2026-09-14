using Photobiz.Application.Common.Interfaces;

namespace Photobiz.Api.Services
{
    /// <summary>
    /// Reads the base URL off the current request. Lives in the web layer (like
    /// <see cref="CurrentUser"/> and <see cref="TenantService"/>) because it needs
    /// <see cref="IHttpContextAccessor"/>.
    /// </summary>
    public sealed class RequestPublicUrlProvider : IPublicUrlProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestPublicUrlProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException("No current HTTP request to resolve a base URL from.");

            return $"{request.Scheme}://{request.Host}";
        }
    }
}

namespace Photobiz.Application.Features.SiteThemes.Common
{
    internal static class FooterLinkUrlValidation
    {
        public static bool IsAbsoluteHttpUrl(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

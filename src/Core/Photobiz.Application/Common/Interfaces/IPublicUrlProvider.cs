namespace Photobiz.Application.Common.Interfaces
{
    /// <summary>
    /// The externally-reachable base URL for the current request (scheme + host + port, no trailing
    /// slash), so a handler can build an absolute URL for a file it just stored — a root-relative
    /// path like "/media/..." would resolve against the browser's own origin (the Angular app),
    /// not this API's, since the two run on different ports in development.
    /// </summary>
    public interface IPublicUrlProvider
    {
        string GetBaseUrl();
    }
}

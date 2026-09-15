using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Photobiz.PortfolioApp.Services;

namespace Photobiz.PortfolioApp.Controllers
{
    /// <summary>Per-tenant robots.txt and sitemap.xml, built from the same site payload every other page uses.</summary>
    public class SeoController : Controller
    {
        private readonly ICurrentSiteProvider _siteProvider;

        public SeoController(ICurrentSiteProvider siteProvider)
        {
            _siteProvider = siteProvider;
        }

        [Route("/robots.txt")]
        public async Task<IActionResult> RobotsTxt(CancellationToken cancellationToken)
        {
            // Resolving the site first means a request to an unconfigured host still gets the
            // shared SiteNotFound handling instead of an always-200 robots.txt for domains that
            // aren't actually a tenant's site.
            await _siteProvider.GetAsync(cancellationToken);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = $"User-agent: *\nAllow: /\n\nSitemap: {baseUrl}/sitemap.xml\n";

            return Content(content, "text/plain", Encoding.UTF8);
        }

        [Route("/sitemap.xml")]
        public async Task<IActionResult> SitemapXml(CancellationToken cancellationToken)
        {
            var site = await _siteProvider.GetAsync(cancellationToken);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urls = new List<XElement>
            {
                UrlEntry(ns, baseUrl, "1.0"),
                UrlEntry(ns, $"{baseUrl}/galleries", "0.8"),
            };
            urls.AddRange(site.Galleries.Select(gallery => UrlEntry(ns, $"{baseUrl}/galleries/{gallery.Id}", "0.6")));

            var document = new XDocument(new XElement(ns + "urlset", urls));

            return Content(document.Declaration + Environment.NewLine + document, "application/xml", Encoding.UTF8);
        }

        private static XElement UrlEntry(XNamespace ns, string loc, string priority) =>
            new(ns + "url", new XElement(ns + "loc", loc), new XElement(ns + "priority", priority));
    }
}

using Microsoft.AspNetCore.Mvc;
using Photobiz.PortfolioApp.Models;
using Photobiz.PortfolioApp.Services;

namespace Photobiz.PortfolioApp.Controllers
{
    [Route("galleries")]
    public class GalleriesController : Controller
    {
        private readonly ICurrentSiteProvider _siteProvider;

        public GalleriesController(ICurrentSiteProvider siteProvider)
        {
            _siteProvider = siteProvider;
        }

        // Explicit attribute routes rather than relying on the default "{controller}/{action=Index}/{id?}"
        // convention route — that convention treats a gallery's Guid segment as the *action name*
        // ("/galleries/{guid}" -> action="{guid}"), not as an id routed to Details, and 404s before
        // either action method ever runs.
        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var site = await _siteProvider.GetAsync(cancellationToken);

            ViewData["Title"] = "Galleries";
            ViewData["MetaDescription"] = $"Browse {site.TenantName}'s photo galleries.";

            return View(site);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var site = await _siteProvider.GetAsync(cancellationToken);
            var gallery = site.Galleries.FirstOrDefault(g => g.Id == id);

            if (gallery is null)
            {
                return NotFound();
            }

            ViewData["Title"] = gallery.Title;
            ViewData["MetaDescription"] = gallery.Description ?? $"{gallery.Title} - {site.TenantName}.";

            return View(new GalleryDetailsViewModel(site, gallery));
        }
    }
}

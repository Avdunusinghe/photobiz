using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Photobiz.PortfolioApp.Models;
using Photobiz.PortfolioApp.Services;

namespace Photobiz.PortfolioApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICurrentSiteProvider _siteProvider;

        public HomeController(ICurrentSiteProvider siteProvider)
        {
            _siteProvider = siteProvider;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var site = await _siteProvider.GetAsync(cancellationToken);

            return View(site);
        }

        /// <summary>
        /// The single target for <c>app.UseExceptionHandler("/error")</c> — re-executed by the
        /// framework for any unhandled exception, with the original exception available via
        /// <see cref="IExceptionHandlerFeature"/>. Branches on exception type rather than having a
        /// separate handler path per exception, since a public site should never leak a stack trace
        /// regardless of what went wrong.
        /// </summary>
        [Route("/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (feature?.Error is SiteNotFoundException)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("SiteNotFound");
            }

            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

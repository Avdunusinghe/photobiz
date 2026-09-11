using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Photobiz.PortfolioSample.Models;
using Photobiz.PortfolioSample.Services;

namespace Photobiz.PortfolioSample.Controllers;

public class HomeController : Controller
{
    private readonly PortfolioApiClient _apiClient;

    public HomeController(PortfolioApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index() => View(new HomeIndexViewModel());

    /// <summary>Simulates a visitor landing on a tenant's public portfolio site.</summary>
    [HttpPost]
    public async Task<IActionResult> CheckTenant(HomeIndexViewModel model, CancellationToken cancellationToken)
    {
        model.TenantCheckResult = await _apiClient.CheckTenantAsync(model.SimulatedHost, cancellationToken);
        return View(nameof(Index), model);
    }

    /// <summary>Simulates an admin signing in — the request-body tenant resolution path.</summary>
    [HttpPost]
    public async Task<IActionResult> Login(HomeIndexViewModel model, CancellationToken cancellationToken)
    {
        model.LoginResult = await _apiClient.LoginAsync(model.TenantKey, model.Username, model.Password, cancellationToken);
        return View(nameof(Index), model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

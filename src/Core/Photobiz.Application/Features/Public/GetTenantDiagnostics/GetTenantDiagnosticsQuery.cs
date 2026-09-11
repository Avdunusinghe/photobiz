using MediatR;

namespace Photobiz.Application.Features.Public.GetTenantDiagnostics
{
    /// <summary>
    /// Reads a small, harmless slice of the <i>currently resolved</i> tenant database — proof that
    /// <c>TenantSelectionMiddleware</c> actually pointed the ambient <c>PhotobizDbContext</c> at the
    /// right tenant before this ran, since two different tenants will answer with different data.
    /// Backs the public, unauthenticated diagnostics endpoint used to test tenant resolution
    /// end-to-end (see <c>PublicController</c> and the Photobiz.PortfolioSample app).
    /// </summary>
    public record GetTenantDiagnosticsQuery : IRequest<TenantDiagnosticsDto>;

    public record TenantDiagnosticsDto(int UserCount, IReadOnlyList<string> SampleUsernames);
}

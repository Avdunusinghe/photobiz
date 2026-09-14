using MediatR;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.GetTenantDetails
{
    /// <summary>Fetches the calling user's own tenant's profile — there is no id parameter, the tenant is always the caller's own (resolved from the JWT's tenant claim).</summary>
    public record GetTenantDetailsQuery : IRequest<TenantDetailsDto>;
}

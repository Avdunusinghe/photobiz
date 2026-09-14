using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.UpdateTenantDetails
{
    /// <summary>
    /// Updates the calling user's own tenant's profile — always the caller's own tenant, resolved
    /// from the JWT's tenant claim. The logo is managed separately via
    /// <c>UploadTenantLogoCommand</c>/<c>RemoveTenantLogoCommand</c>.
    /// </summary>
    public record UpdateTenantDetailsCommand(
        string Name,
        string CustomerEmail,
        string CustomerFirstName,
        string CustomerLastName,
        string Phone,
        string Address,
        string City,
        string Country) : IRequest<ResultDto<TenantDetailsDto>>;
}

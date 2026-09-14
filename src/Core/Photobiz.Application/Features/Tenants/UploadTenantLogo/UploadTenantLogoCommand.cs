using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.UploadTenantLogo
{
    /// <summary>Uploads (or replaces) the calling user's own tenant's logo — always the caller's own tenant, resolved from the JWT's tenant claim.</summary>
    public record UploadTenantLogoCommand(
        Stream Content,
        string FileName,
        string ContentType,
        long Length) : IRequest<ResultDto<TenantDetailsDto>>;
}

using MediatR;
using Photobiz.Application.Features.SmtpSettings.Common;

namespace Photobiz.Application.Features.SmtpSettings.GetSmtpSettings
{
    /// <summary>
    /// Fetches the calling user's own tenant's SMTP configuration — there is no id parameter, the
    /// tenant is always the caller's own (resolved from the JWT's tenant claim). Returns <c>null</c>
    /// when the tenant hasn't configured one yet, rather than throwing — that's a normal, expected
    /// state (the platform's default sender is used instead), not an error.
    /// </summary>
    public record GetSmtpSettingsQuery : IRequest<SmtpSettingDto?>;
}

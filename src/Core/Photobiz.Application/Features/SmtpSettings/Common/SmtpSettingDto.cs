namespace Photobiz.Application.Features.SmtpSettings.Common
{
    /// <summary>
    /// The current tenant's own outgoing-mail configuration, as shown on their "Business Profile"
    /// screen. Deliberately excludes <c>Password</c> — a credential is never round-tripped back to
    /// the client once saved.
    /// </summary>
    public record SmtpSettingDto(
        Guid Id,
        string Host,
        int Port,
        string Username,
        bool EnableSsl,
        string FromEmail,
        string? FromName,
        bool IsEnabled,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}

namespace Photobiz.Application.Common.Interfaces
{
    /// <summary>
    /// Ambient information about the caller behind the current request.
    /// Implemented in the web layer from the authenticated principal; resolves to
    /// <c>null</c> for background work (migrations, seeding, scheduled jobs).
    /// </summary>
    public interface ICurrentUser
    {
        string? UserName { get; }
    }
}

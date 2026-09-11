namespace Photobiz.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when a request can't be routed to a tenant database: an unknown/blank tenant key
    /// at login, or a JWT whose tenant claim no longer resolves to a registered tenant.
    /// </summary>
    public class TenantNotFoundException : Exception
    {
        public TenantNotFoundException(string message) : base(message)
        {
        }
    }
}

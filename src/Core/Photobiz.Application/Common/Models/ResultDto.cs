namespace Photobiz.Application.Common.Models
{
    /// <summary>
    /// Standard response envelope returned to clients for create / update / delete operations.
    /// Failure paths (validation, not found, conflict) are surfaced as RFC 9110 ProblemDetails
    /// by the global exception handler, so a <see cref="ResultDto"/> that reaches the client
    /// always represents a completed operation.
    /// </summary>
    public record ResultDto(bool Success, string? Message = null)
    {
        public static ResultDto Succeeded(string? message = null) => new(true, message);

        public static ResultDto Failed(string message) => new(false, message);
    }

    /// <summary>
    /// <see cref="ResultDto"/> that also carries a payload (for example the affected resource).
    /// </summary>
    public record ResultDto<T>(bool Success, T? Data, string? Message = null)
        : ResultDto(Success, Message)
    {
        public static ResultDto<T> Succeeded(T data, string? message = null) => new(true, data, message);

        public static new ResultDto<T> Failed(string message) => new(false, default, message);
    }
}

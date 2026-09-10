namespace Photobiz.Application.Features.Users.Common
{
    public record UserDto(
        Guid Id,
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string? MobileNumber,
        bool IsActive,
        DateTime CreatedAt,
        IReadOnlyList<string> Roles);
}

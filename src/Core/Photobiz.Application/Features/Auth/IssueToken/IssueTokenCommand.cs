using MediatR;

namespace Photobiz.Application.Features.Auth.IssueToken
{
    public record IssueTokenCommand(string Username, string Password) : IRequest<IssueTokenResult>;

    public record IssueTokenResult(string AccessToken, DateTime ExpiresAtUtc);
}

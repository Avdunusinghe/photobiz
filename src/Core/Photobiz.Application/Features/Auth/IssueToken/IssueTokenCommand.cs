using MediatR;

namespace Photobiz.Application.Features.Auth.IssueToken
{
    public record IssueTokenCommand(string Username) : IRequest<IssueTokenResult>;

    public record IssueTokenResult(string AccessToken, DateTime ExpiresAtUtc);
}

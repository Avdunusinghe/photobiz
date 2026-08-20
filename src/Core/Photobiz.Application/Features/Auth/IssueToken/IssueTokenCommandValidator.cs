using FluentValidation;

namespace Photobiz.Application.Features.Auth.IssueToken
{
    public class IssueTokenCommandValidator : AbstractValidator<IssueTokenCommand>
    {
        public IssueTokenCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(256);
        }
    }
}

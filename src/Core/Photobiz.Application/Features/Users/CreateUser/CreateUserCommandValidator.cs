using FluentValidation;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(128);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.MobileNumber)
                .MaximumLength(32)
                .Matches(@"^\+?[0-9\s\-()]{7,32}$")
                .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

            RuleFor(x => x.Roles)
                .NotEmpty().WithMessage("At least one role is required.");

            RuleForEach(x => x.Roles)
                .Must(role => RoleNames.All.Contains(role))
                .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
        }
    }
}

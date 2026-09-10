using FluentValidation;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.GetUsers
{
    public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
    {
        public const int MaxPageSize = 100;

        public GetUsersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, MaxPageSize);

            RuleFor(x => x.SearchText)
                .MaximumLength(256)
                .When(x => x.SearchText is not null);

            RuleFor(x => x.Role)
                .Must(role => RoleNames.All.Contains(role!))
                .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.Role));
        }
    }
}

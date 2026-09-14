using FluentValidation;

namespace Photobiz.Application.Features.SiteThemes.ReorderFooterLinks
{
    public class ReorderFooterLinksCommandValidator : AbstractValidator<ReorderFooterLinksCommand>
    {
        public ReorderFooterLinksCommandValidator()
        {
            RuleFor(x => x.OrderedFooterLinkIds).NotEmpty();

            RuleFor(x => x.OrderedFooterLinkIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("The footer link order cannot contain duplicates.")
                .When(x => x.OrderedFooterLinkIds.Count > 0);
        }
    }
}

using FluentValidation;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.UpdateFooterLink
{
    public class UpdateFooterLinkCommandValidator : AbstractValidator<UpdateFooterLinkCommand>
    {
        public UpdateFooterLinkCommandValidator()
        {
            RuleFor(x => x.Platform).IsInEnum();

            RuleFor(x => x.Url)
                .NotEmpty()
                .Must(FooterLinkUrlValidation.IsAbsoluteHttpUrl)
                .WithMessage("Enter a valid http(s) URL.");
        }
    }
}

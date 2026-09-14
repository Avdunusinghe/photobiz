using FluentValidation;

namespace Photobiz.Application.Features.Tenants.UpdateTenantDetails
{
    public class UpdateTenantDetailsCommandValidator : AbstractValidator<UpdateTenantDetailsCommand>
    {
        public UpdateTenantDetailsCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.CustomerEmail)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.CustomerFirstName)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.CustomerLastName)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .MaximumLength(32);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(128);

            RuleFor(x => x.Country)
                .NotEmpty()
                .MaximumLength(128);
        }
    }
}

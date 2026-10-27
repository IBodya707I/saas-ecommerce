namespace Reviews.Application.Storefronts.Commands.UpdateStorefrontBranding;

using FluentValidation;

public class UpdateStorefrontBrandingCommandValidator : AbstractValidator<UpdateStorefrontBrandingCommand>
{
    public UpdateStorefrontBrandingCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("StoreId is required.");

        RuleFor(x => x.Color)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Color is required.");

        RuleFor(x => x.FontFamily)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("FontFamily is required.");
    }
}

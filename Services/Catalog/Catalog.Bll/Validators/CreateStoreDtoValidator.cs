using Catalog.Bll.Dtos;
using FluentValidation;

namespace Catalog.Bll.Validators;

public class CreateStoreDtoValidator : AbstractValidator<CreateStoreDto>
{
    public CreateStoreDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(150).WithMessage("Store name cannot exceed 150 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Store slug is required")
            .MaximumLength(100).WithMessage("Store slug cannot exceed 100 characters")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Store slug must be lowercase alphanumeric with hyphens");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Store category is required")
            .MaximumLength(100).WithMessage("Store category cannot exceed 100 characters");
    }
}

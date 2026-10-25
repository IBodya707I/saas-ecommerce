using Catalog.Bll.Dtos;
using FluentValidation;

namespace Catalog.Bll.Validators;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Category slug is required")
            .MaximumLength(100).WithMessage("Category slug cannot exceed 100 characters")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Category slug must be lowercase alphanumeric with hyphens");
    }
}

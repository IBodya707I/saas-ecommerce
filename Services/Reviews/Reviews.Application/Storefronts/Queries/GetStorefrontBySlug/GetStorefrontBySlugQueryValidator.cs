namespace Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

using FluentValidation;

public class GetStorefrontBySlugQueryValidator : AbstractValidator<GetStorefrontBySlugQuery>
{
    public GetStorefrontBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Slug is required.");
    }
}

namespace Reviews.Application.Queries.GetReviewsList;

using FluentValidation;

public class GetReviewsListQueryValidator : AbstractValidator<GetReviewsListQuery>
{
    public GetReviewsListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        When(x => x.ProductId.HasValue, () =>
        {
            RuleFor(x => x.ProductId!.Value)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than zero.");
        });
    }
}

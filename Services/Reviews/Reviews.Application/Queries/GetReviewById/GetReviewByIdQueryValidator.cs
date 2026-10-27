namespace Reviews.Application.Queries.GetReviewById;

using FluentValidation;
using MongoDB.Bson;

public class GetReviewByIdQueryValidator : AbstractValidator<GetReviewByIdQuery>
{
    public GetReviewByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid ObjectId.");
    }
}

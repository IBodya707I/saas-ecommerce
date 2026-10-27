namespace Reviews.Application.Commands.DeleteReview;

using FluentValidation;
using MongoDB.Bson;

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid ObjectId.");
    }
}

namespace Reviews.Application.Commands.UpdateReview;

using FluentValidation;
using MongoDB.Bson;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid ObjectId.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Review text cannot be empty or exceed 2000 characters.");
    }
}

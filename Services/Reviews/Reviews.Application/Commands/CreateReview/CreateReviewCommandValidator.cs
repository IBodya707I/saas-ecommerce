namespace Reviews.Application.Commands.CreateReview;

using FluentValidation;
using Reviews.Domain.Interfaces;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator(IReviewRepository repository)
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("ProductId must be greater than zero.");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("AuthorId is required.");

        RuleFor(x => x.AuthorName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("AuthorName is required.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Review text cannot be empty or exceed 2000 characters.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var exists = await repository.ExistsAsync(cmd.ProductId, cmd.AuthorId, ct);
                return !exists;
            })
            .WithMessage("You have already reviewed this product.");
    }
}

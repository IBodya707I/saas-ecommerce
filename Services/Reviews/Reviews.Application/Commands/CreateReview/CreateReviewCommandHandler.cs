namespace Reviews.Application.Commands.CreateReview;

using MediatR;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Domain.ValueObjects;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, string>
{
    private readonly IReviewRepository _repository;

    public CreateReviewCommandHandler(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(CreateReviewCommand cmd, CancellationToken ct)
    {
        var author = new Author(cmd.AuthorId, cmd.AuthorName);
        var review = new Review(cmd.ProductId, cmd.ProductName, author, cmd.Rating, cmd.Text);

        await _repository.AddAsync(review, ct);
        return review.Id.ToString();
    }
}

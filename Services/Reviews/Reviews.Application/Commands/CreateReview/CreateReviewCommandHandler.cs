namespace Reviews.Application.Commands.CreateReview;

using MediatR;
using Reviews.Application.Caching;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Domain.ValueObjects;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, string>
{
    private readonly IReviewRepository _repository;
    private readonly IReviewsCacheService _cache;

    public CreateReviewCommandHandler(IReviewRepository repository, IReviewsCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<string> Handle(CreateReviewCommand cmd, CancellationToken ct)
    {
        var author = new Author(cmd.AuthorId, cmd.AuthorName);
        var review = new Review(cmd.ProductId, cmd.ProductName, author, cmd.Rating, cmd.Text);

        await _repository.AddAsync(review, ct);
        await _cache.IncrementVersionAsync($"reviews:ver:{cmd.ProductId}", ct);
        await _cache.IncrementVersionAsync("reviews:ver:all", ct);

        return review.Id.ToString();
    }
}

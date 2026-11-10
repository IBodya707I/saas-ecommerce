namespace Reviews.Application.Commands.UpdateReview;

using MediatR;
using MongoDB.Bson;
using Reviews.Application.Caching;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly IReviewRepository _repository;
    private readonly IReviewsCacheService _cache;

    public UpdateReviewCommandHandler(IReviewRepository repository, IReviewsCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task Handle(UpdateReviewCommand cmd, CancellationToken ct)
    {
        if (!ObjectId.TryParse(cmd.Id, out var objectId))
            throw new NotFoundException(nameof(Review), cmd.Id);

        var review = await _repository.GetByIdAsync(objectId, ct)
            ?? throw new NotFoundException(nameof(Review), cmd.Id);

        review.Update(cmd.Rating, cmd.Text);
        await _repository.UpdateAsync(review, ct);
        await _cache.IncrementVersionAsync($"reviews:ver:{review.ProductId}", ct);
        await _cache.IncrementVersionAsync("reviews:ver:all", ct);
    }
}

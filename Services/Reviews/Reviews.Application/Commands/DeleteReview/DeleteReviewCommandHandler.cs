namespace Reviews.Application.Commands.DeleteReview;

using MediatR;
using MongoDB.Bson;
using Reviews.Application.Caching;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _repository;
    private readonly IReviewsCacheService _cache;

    public DeleteReviewCommandHandler(IReviewRepository repository, IReviewsCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task Handle(DeleteReviewCommand cmd, CancellationToken ct)
    {
        if (!ObjectId.TryParse(cmd.Id, out var objectId))
            throw new NotFoundException(nameof(Review), cmd.Id);

        var review = await _repository.GetByIdAsync(objectId, ct)
            ?? throw new NotFoundException(nameof(Review), cmd.Id);

        await _repository.DeleteAsync(objectId, ct);
        await _cache.RemoveAsync($"reviews:product:{review.ProductId}:1:20", "ReviewDeleted", ct);
    }
}

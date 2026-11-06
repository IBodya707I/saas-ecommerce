namespace Reviews.Infrastructure.Repositories;

using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Infrastructure.Persistence;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepository(MongoDbContext context)
    {
        _reviews = context.Reviews;
    }

    public async Task<Review?> GetByIdAsync(ObjectId id, CancellationToken ct = default)
    {
        return await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(long productId, string authorId, CancellationToken ct = default)
    {
        return await _reviews
            .Find(r => r.ProductId == productId && r.Author.UserId == authorId)
            .AnyAsync(ct);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default)
    {
        await _reviews.InsertOneAsync(review, cancellationToken: ct);
    }

    public async Task UpdateAsync(Review review, CancellationToken ct = default)
    {
        var expectedVersion = review.Version;
        review.BumpVersion();

        var result = await _reviews.ReplaceOneAsync(
            r => r.Id == review.Id && r.Version == expectedVersion,
            review,
            cancellationToken: ct);

        if (result.MatchedCount == 0)
            throw new ConflictException($"Review with id '{review.Id}' was modified concurrently.");
    }

    public async Task DeleteAsync(ObjectId id, CancellationToken ct = default)
    {
        await _reviews.DeleteOneAsync(r => r.Id == id, ct);
    }
}

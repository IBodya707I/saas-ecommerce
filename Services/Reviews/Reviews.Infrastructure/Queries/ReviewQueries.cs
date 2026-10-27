namespace Reviews.Infrastructure.Queries;

using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

public class ReviewQueries : IReviewQueries
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewQueries(MongoDbContext context)
    {
        _reviews = context.Reviews;
    }

    public async Task<ReviewDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        return await _reviews.Find(r => r.Id == objectId)
            .Project(r => new ReviewDto(
                r.Id.ToString(),
                r.ProductId,
                r.ProductName,
                r.Author != null ? r.Author.DisplayName : string.Empty,
                r.Rating,
                r.Text,
                r.Comments != null ? r.Comments.Count : 0,
                r.Version,
                r.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ReviewDto>> GetListAsync(long? productId, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = productId.HasValue
            ? Builders<Review>.Filter.Eq(r => r.ProductId, productId.Value)
            : Builders<Review>.Filter.Empty;

        var totalCount = await _reviews.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await _reviews.Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Project(r => new ReviewDto(
                r.Id.ToString(),
                r.ProductId,
                r.ProductName,
                r.Author != null ? r.Author.DisplayName : string.Empty,
                r.Rating,
                r.Text,
                r.Comments != null ? r.Comments.Count : 0,
                r.Version,
                r.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ReviewDto>(items, totalCount, page, pageSize);
    }
}

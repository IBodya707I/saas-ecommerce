namespace Reviews.Infrastructure.Queries;

using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

public class ReviewQueries : IReviewQueries
{
    private readonly IMongoCollection<Review> _reviews;
    private readonly IMapper _mapper;

    public ReviewQueries(MongoDbContext context, IMapper mapper)
    {
        _reviews = context.Reviews;
        _mapper = mapper;
    }

    public async Task<ReviewDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        var review = await _reviews.Find(r => r.Id == objectId).FirstOrDefaultAsync(ct);
        return review == null ? null : _mapper.Map<ReviewDto>(review);
    }

    public async Task<PagedResult<ReviewDto>> GetListAsync(long? productId, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = productId.HasValue
            ? Builders<Review>.Filter.Eq(r => r.ProductId, productId.Value)
            : Builders<Review>.Filter.Empty;

        var totalCount = await _reviews.CountDocumentsAsync(filter, cancellationToken: ct);

        var reviews = await _reviews.Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var dtos = _mapper.Map<List<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>(dtos, totalCount, page, pageSize);
    }
}

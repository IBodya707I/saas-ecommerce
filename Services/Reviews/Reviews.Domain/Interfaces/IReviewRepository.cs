namespace Reviews.Domain.Interfaces;

using MongoDB.Bson;
using Reviews.Domain.Entities;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(ObjectId id, CancellationToken ct = default);
    Task<bool> ExistsAsync(long productId, string authorId, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    Task UpdateAsync(Review review, CancellationToken ct = default);
    Task DeleteAsync(ObjectId id, CancellationToken ct = default);
}

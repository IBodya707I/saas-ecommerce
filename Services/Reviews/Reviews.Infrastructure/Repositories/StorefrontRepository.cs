namespace Reviews.Infrastructure.Repositories;

using MongoDB.Driver;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Infrastructure.Persistence;

public class StorefrontRepository : IStorefrontRepository
{
    private readonly IMongoCollection<StorefrontShowcase> _showcases;

    public StorefrontRepository(MongoDbContext context)
    {
        _showcases = context.StorefrontShowcases;
    }

    public async Task<StorefrontShowcase?> GetByStoreIdAsync(string storeId, CancellationToken ct = default)
    {
        return await _showcases.Find(s => s.StoreId == storeId).FirstOrDefaultAsync(ct);
    }

    public async Task<StorefrontShowcase?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _showcases.Find(s => s.Slug == slug.ToLowerInvariant()).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(StorefrontShowcase storefront, CancellationToken ct = default)
    {
        await _showcases.InsertOneAsync(storefront, cancellationToken: ct);
    }

    public async Task UpdateAsync(StorefrontShowcase storefront, CancellationToken ct = default)
    {
        var expectedVersion = storefront.Version;
        storefront.BumpVersion();

        var result = await _showcases.ReplaceOneAsync(
            s => s.Id == storefront.Id && s.Version == expectedVersion,
            storefront,
            cancellationToken: ct);

        if (result.MatchedCount == 0)
            throw new ConflictException($"Storefront with id '{storefront.Id}' was modified concurrently.");
    }
}

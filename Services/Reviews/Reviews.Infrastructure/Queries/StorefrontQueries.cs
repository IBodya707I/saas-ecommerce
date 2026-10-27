namespace Reviews.Infrastructure.Queries;

using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

public class StorefrontQueries : IStorefrontQueries
{
    private readonly IMongoCollection<StorefrontShowcase> _showcases;

    public StorefrontQueries(MongoDbContext context)
    {
        _showcases = context.StorefrontShowcases;
    }

    public async Task<StorefrontDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var lowerSlug = slug.ToLowerInvariant();

        return await _showcases.Find(s => s.Slug == lowerSlug)
            .Project(s => new StorefrontDto(
                s.Id.ToString(),
                s.StoreId,
                s.Slug,
                s.StoreName,
                s.Branding != null ? s.Branding.Color : string.Empty,
                s.Branding != null ? s.Branding.FontFamily : string.Empty,
                s.Branding != null ? s.Branding.LogoUrl : null,
                s.WorkSchedule))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StorefrontDto?> GetByStoreIdAsync(string storeId, CancellationToken ct = default)
    {
        return await _showcases.Find(s => s.StoreId == storeId)
            .Project(s => new StorefrontDto(
                s.Id.ToString(),
                s.StoreId,
                s.Slug,
                s.StoreName,
                s.Branding != null ? s.Branding.Color : string.Empty,
                s.Branding != null ? s.Branding.FontFamily : string.Empty,
                s.Branding != null ? s.Branding.LogoUrl : null,
                s.WorkSchedule))
            .FirstOrDefaultAsync(ct);
    }
}

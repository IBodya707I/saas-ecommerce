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

        var doc = await _showcases.Find(s => s.Slug == lowerSlug).FirstOrDefaultAsync(ct);
        if (doc is null)
            return null;

        return new StorefrontDto(
            doc.Id.ToString(),
            doc.StoreId,
            doc.Slug,
            doc.StoreName,
            doc.Branding != null ? doc.Branding.Color : string.Empty,
            doc.Branding != null ? doc.Branding.FontFamily : string.Empty,
            doc.Branding != null ? doc.Branding.LogoUrl : null,
            doc.WorkSchedule);
    }

    public async Task<StorefrontDto?> GetByStoreIdAsync(string storeId, CancellationToken ct = default)
    {
        var doc = await _showcases.Find(s => s.StoreId == storeId).FirstOrDefaultAsync(ct);
        if (doc is null)
            return null;

        return new StorefrontDto(
            doc.Id.ToString(),
            doc.StoreId,
            doc.Slug,
            doc.StoreName,
            doc.Branding != null ? doc.Branding.Color : string.Empty,
            doc.Branding != null ? doc.Branding.FontFamily : string.Empty,
            doc.Branding != null ? doc.Branding.LogoUrl : null,
            doc.WorkSchedule);
    }
}

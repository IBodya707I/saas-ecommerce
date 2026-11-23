namespace Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

using MediatR;
using Reviews.Application.Caching;
using Reviews.Application.Common.Exceptions;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

public class GetStorefrontBySlugQueryHandler : IRequestHandler<GetStorefrontBySlugQuery, StorefrontDto>
{
    private readonly IStorefrontQueries _queries;
    private readonly IReviewsCacheService _cache;

    public GetStorefrontBySlugQueryHandler(IStorefrontQueries queries, IReviewsCacheService cache)
    {
        _queries = queries;
        _cache = cache;
    }

    public async Task<StorefrontDto> Handle(GetStorefrontBySlugQuery query, CancellationToken ct)
    {
        var key = $"storefront:{query.Slug.ToLowerInvariant()}";
        var dto = await _cache.GetOrCreateAsync(
            key,
            async () =>
            {
                var result = await _queries.GetBySlugAsync(query.Slug, ct);
                if (result is null)
                    throw new NotFoundException(nameof(StorefrontShowcase), query.Slug);
                return result;
            },
            l1Expiration: TimeSpan.FromMinutes(2),
            l2Expiration: TimeSpan.FromMinutes(15),
            ct: ct);

        return dto ?? throw new NotFoundException(nameof(StorefrontShowcase), query.Slug);
    }
}

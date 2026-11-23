namespace Reviews.Application.Queries.GetReviewsList;

using MediatR;
using Reviews.Application.Caching;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

public class GetReviewsListQueryHandler : IRequestHandler<GetReviewsListQuery, PagedResult<ReviewDto>>
{
    private readonly IReviewQueries _queries;
    private readonly IReviewsCacheService _cache;

    public GetReviewsListQueryHandler(IReviewQueries queries, IReviewsCacheService cache)
    {
        _queries = queries;
        _cache = cache;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsListQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        string key;
        if (query.ProductId.HasValue)
        {
            var prodVer = await _cache.GetVersionAsync($"reviews:ver:{query.ProductId.Value}", ct);
            key = $"reviews:product:{query.ProductId}:{prodVer}:{page}:{pageSize}";
        }
        else
        {
            var allVer = await _cache.GetVersionAsync("reviews:ver:all", ct);
            key = $"reviews:list:{allVer}:{page}:{pageSize}";
        }

        var result = await _cache.GetOrCreateAsync(
            key,
            () => _queries.GetListAsync(query.ProductId, page, pageSize, ct)!,
            l1Expiration: TimeSpan.FromMinutes(1),
            l2Expiration: TimeSpan.FromMinutes(5),
            ct: ct);

        return result ?? new PagedResult<ReviewDto>(Array.Empty<ReviewDto>(), 0, page, pageSize);
    }
}

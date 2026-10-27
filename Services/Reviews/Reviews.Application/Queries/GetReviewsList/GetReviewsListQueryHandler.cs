namespace Reviews.Application.Queries.GetReviewsList;

using MediatR;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

public class GetReviewsListQueryHandler : IRequestHandler<GetReviewsListQuery, PagedResult<ReviewDto>>
{
    private readonly IReviewQueries _queries;

    public GetReviewsListQueryHandler(IReviewQueries queries)
    {
        _queries = queries;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsListQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        return await _queries.GetListAsync(query.ProductId, page, pageSize, ct);
    }
}

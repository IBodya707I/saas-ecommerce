namespace Reviews.Application.Queries.GetReviewsList;

using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

public record GetReviewsListQuery(
    long? ProductId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ReviewDto>>;

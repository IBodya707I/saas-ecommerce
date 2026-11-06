namespace Reviews.Application.Queries.GetReviewById;

using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

public record GetReviewByIdQuery(string Id) : IQuery<ReviewDto>;

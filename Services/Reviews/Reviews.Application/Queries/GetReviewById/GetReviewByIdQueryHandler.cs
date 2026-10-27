namespace Reviews.Application.Queries.GetReviewById;

using MediatR;
using Reviews.Application.Common.Exceptions;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ReviewDto>
{
    private readonly IReviewQueries _queries;

    public GetReviewByIdQueryHandler(IReviewQueries queries)
    {
        _queries = queries;
    }

    public async Task<ReviewDto> Handle(GetReviewByIdQuery query, CancellationToken ct)
    {
        var dto = await _queries.GetByIdAsync(query.Id, ct);
        if (dto is null)
            throw new NotFoundException(nameof(Review), query.Id);

        return dto;
    }
}

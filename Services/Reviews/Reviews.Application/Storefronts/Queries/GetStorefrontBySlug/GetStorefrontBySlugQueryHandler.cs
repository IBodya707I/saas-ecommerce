namespace Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

using MediatR;
using Reviews.Application.Common.Exceptions;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

public class GetStorefrontBySlugQueryHandler : IRequestHandler<GetStorefrontBySlugQuery, StorefrontDto>
{
    private readonly IStorefrontQueries _queries;

    public GetStorefrontBySlugQueryHandler(IStorefrontQueries queries)
    {
        _queries = queries;
    }

    public async Task<StorefrontDto> Handle(GetStorefrontBySlugQuery query, CancellationToken ct)
    {
        var dto = await _queries.GetBySlugAsync(query.Slug, ct);
        if (dto is null)
            throw new NotFoundException(nameof(StorefrontShowcase), query.Slug);

        return dto;
    }
}

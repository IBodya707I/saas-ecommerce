namespace Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

public record GetStorefrontBySlugQuery(string Slug) : IQuery<StorefrontDto>;

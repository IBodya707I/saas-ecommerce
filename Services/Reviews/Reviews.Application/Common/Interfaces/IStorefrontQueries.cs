namespace Reviews.Application.Common.Interfaces;

using Reviews.Application.Common.Models;

public interface IStorefrontQueries
{
    Task<StorefrontDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<StorefrontDto?> GetByStoreIdAsync(string storeId, CancellationToken ct = default);
}

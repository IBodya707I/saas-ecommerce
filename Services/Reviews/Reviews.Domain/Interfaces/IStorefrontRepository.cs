namespace Reviews.Domain.Interfaces;

using Reviews.Domain.Entities;

public interface IStorefrontRepository
{
    Task<StorefrontShowcase?> GetByStoreIdAsync(string storeId, CancellationToken ct = default);
    Task<StorefrontShowcase?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(StorefrontShowcase storefront, CancellationToken ct = default);
    Task UpdateAsync(StorefrontShowcase storefront, CancellationToken ct = default);
}

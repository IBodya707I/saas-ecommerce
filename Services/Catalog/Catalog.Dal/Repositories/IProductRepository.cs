using Catalog.Domain.Entities;

namespace Catalog.Dal.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsBySkuAsync(Guid storeId, string sku, CancellationToken ct = default);
}

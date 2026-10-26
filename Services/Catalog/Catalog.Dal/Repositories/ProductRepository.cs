using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(CatalogDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await Set
            .Include(p => p.Detail)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Category)
                .ThenInclude(c => c!.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }

    public async Task<bool> ExistsBySkuAsync(Guid storeId, string sku, CancellationToken ct = default)
    {
        return await Set.AnyAsync(
            p => p.StoreId == storeId && p.Sku == sku.Trim().ToUpperInvariant() && !p.IsDeleted,
            ct);
    }
}

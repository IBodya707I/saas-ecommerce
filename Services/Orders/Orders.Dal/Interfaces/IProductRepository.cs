using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Domain.Entities;

namespace Orders.Dal.Interfaces;

public interface IProductRepository : IGenericRepository<ProductLocal>
{
    Task<IReadOnlyList<ProductLocal>> GetActiveByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task AdjustStockAsync(Guid storeId, Guid productId, int delta, string reason, CancellationToken ct = default);
    Task SyncProductAsync(ProductLocal product, CancellationToken ct = default);
    Task DecreaseStockAsync(Guid storeId, Guid productId, int quantity, CancellationToken ct = default);
}

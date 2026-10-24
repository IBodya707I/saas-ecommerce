using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Orders.Dal.Common;
using Orders.Dal.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Dal.Repositories;

/// <summary>
/// Product catalog replica repository implemented using Dapper.
/// Supports stock deduction, stock adjustments, and replication synchronization via stored procedures.
/// </summary>
public sealed class ProductRepository : BaseDapperRepository<ProductLocal>, IProductRepository
{
    public ProductRepository(NpgsqlConnection connection, Func<NpgsqlTransaction?> currentTransaction)
        : base(connection, currentTransaction, "products_local")
    {
    }

    public override async Task<Guid> AddAsync(ProductLocal entity, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await EnsureOpenAsync(ct);
            const string sql = @"
                INSERT INTO products_local (id, store_id, name, price, stock, is_active, source_updated_at, created_by)
                VALUES (@Id, @StoreId, @Name, @Price, @Stock, @IsActive, @SourceUpdatedAt, @CreatedBy);";

            var cmd = new CommandDefinition(sql, entity, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(cmd);
            return entity.Id;
        });
    }

    public async Task<IReadOnlyList<ProductLocal>> GetActiveByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                SELECT id, store_id, name, price, stock, is_active, source_updated_at, 
                       created_at, created_by, updated_at, updated_by, is_deleted
                FROM products_local
                WHERE store_id = @StoreId AND is_active = true AND is_deleted = false
                ORDER BY name ASC;";

            var cmd = new CommandDefinition(sql, new { StoreId = storeId }, Transaction, cancellationToken: ct);
            var rows = await Connection.QueryAsync<ProductLocal>(cmd);
            return rows.AsList();
        });
    }

    public async Task AdjustStockAsync(Guid storeId, Guid productId, int delta, string reason, CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                CALL usp_adjust_stock(@StoreId, @ProductId, @Delta, @Reason);";

            var cmd = new CommandDefinition(
                sql,
                new { StoreId = storeId, ProductId = productId, Delta = delta, Reason = reason },
                Transaction,
                cancellationToken: ct);

            await Connection.ExecuteAsync(cmd);
        });
    }

    public async Task SyncProductAsync(ProductLocal product, CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                CALL usp_sync_product(@Id, @StoreId, @Name, @Price, @IsActive, @SourceUpdatedAt);";

            var cmd = new CommandDefinition(
                sql,
                new
                {
                    Id = product.Id,
                    StoreId = product.StoreId,
                    Name = product.Name,
                    Price = product.Price,
                    IsActive = product.IsActive,
                    SourceUpdatedAt = product.SourceUpdatedAt ?? DateTime.UtcNow
                },
                Transaction,
                cancellationToken: ct);

            await Connection.ExecuteAsync(cmd);
        });
    }

    public async Task DecreaseStockAsync(Guid storeId, Guid productId, int quantity, CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            // Atomic stock deduction with condition to prevent negative stock
            const string sql = @"
                UPDATE products_local
                SET stock = stock - @Quantity,
                    updated_at = CURRENT_TIMESTAMP,
                    updated_by = 'decrease_stock'
                WHERE store_id = @StoreId AND id = @ProductId AND stock >= @Quantity AND is_deleted = false;";

            var cmd = new CommandDefinition(
                sql,
                new { StoreId = storeId, ProductId = productId, Quantity = quantity },
                Transaction,
                cancellationToken: ct);

            var affected = await Connection.ExecuteAsync(cmd);
            if (affected == 0)
            {
                throw new BusinessConflictException($"Insufficient stock for product {productId}");
            }
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Orders.Dal.Common;
using Orders.Dal.Interfaces;
using Orders.Domain.Entities;

namespace Orders.Dal.Repositories;

/// <summary>
/// Order repository implemented using Dapper.
/// Supports multi-mapping (Order + OrderDetails + OrderItems in a single query)
/// and stored procedure executions (usp_create_order and usp_change_order_status).
/// </summary>
public sealed class OrderRepository : BaseDapperRepository<Order>, IOrderRepository
{
    public OrderRepository(NpgsqlConnection connection, Func<NpgsqlTransaction?> currentTransaction)
        : base(connection, currentTransaction, "orders")
    {
    }

    public override async Task<Guid> AddAsync(Order entity, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await EnsureOpenAsync(ct);
            const string sql = @"
                INSERT INTO orders (id, customer_id, store_id, status, payment_method, total_amount, version, created_by)
                VALUES (@Id, @CustomerId, @StoreId, @Status, @PaymentMethod, @TotalAmount, @Version, @CreatedBy);";

            var cmd = new CommandDefinition(sql, entity, Transaction, cancellationToken: ct);
            await Connection.ExecuteAsync(cmd);
            return entity.Id;
        });
    }

    public async Task<Order?> GetWithItemsAsync(Guid orderId, Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                SELECT 
                    o.id, o.customer_id, o.store_id, o.status, o.payment_method, o.total_amount, 
                    o.version, o.created_at, o.created_by, o.updated_at, o.updated_by, o.is_deleted,
                    d.order_id, d.delivery_provider, d.city, d.warehouse_address, d.recipient_phone, 
                    d.notes, d.created_at, d.created_by, d.updated_at, d.updated_by, d.is_deleted,
                    i.order_id, i.store_id, i.product_id, i.product_name, i.unit_price, 
                    i.quantity, i.created_at, i.created_by, i.updated_at, i.updated_by, i.is_deleted
                FROM orders o
                LEFT JOIN order_details d ON d.order_id = o.id AND d.is_deleted = false
                LEFT JOIN order_items i ON i.order_id = o.id AND i.is_deleted = false
                WHERE o.id = @OrderId AND o.store_id = @StoreId AND o.is_deleted = false;";

            Order? root = null;
            var cmd = new CommandDefinition(
                sql,
                new { OrderId = orderId, StoreId = storeId },
                Transaction,
                cancellationToken: ct);

            await Connection.QueryAsync<Order, OrderDetails?, OrderItem?, Order>(
                cmd,
                (order, details, item) =>
                {
                    if (root is null)
                    {
                        root = order;
                        root.Details = details;
                    }

                    if (item is not null && !root.Items.Any(x => x.ProductId == item.ProductId))
                    {
                        root.Items.Add(item);
                    }

                    return root;
                },
                splitOn: "order_id,order_id");

            return root;
        });
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                SELECT id, customer_id, store_id, status, payment_method, total_amount, 
                       version, created_at, created_by, updated_at, updated_by, is_deleted
                FROM orders
                WHERE customer_id = @CustomerId AND is_deleted = false
                ORDER BY created_at DESC;";

            var cmd = new CommandDefinition(sql, new { CustomerId = customerId }, Transaction, cancellationToken: ct);
            var result = await Connection.QueryAsync<Order>(cmd);
            return result.AsList();
        });
    }

    public async Task<IReadOnlyList<Order>> GetByStoreAndStatusAsync(Guid storeId, string status, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                SELECT id, customer_id, store_id, status, payment_method, total_amount, 
                       version, created_at, created_by, updated_at, updated_by, is_deleted
                FROM orders
                WHERE store_id = @StoreId AND status = @Status AND is_deleted = false
                ORDER BY created_at DESC;";

            var cmd = new CommandDefinition(sql, new { StoreId = storeId, Status = status }, Transaction, cancellationToken: ct);
            var result = await Connection.QueryAsync<Order>(cmd);
            return result.AsList();
        });
    }

    public async Task ChangeStatusAsync(
        Guid orderId,
        Guid storeId,
        int expectedVersion,
        string newStatus,
        string changedBy,
        string? comment = null,
        CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
                CALL usp_change_order_status(@OrderId, @StoreId, @ExpectedVer, @NewStatus, @ChangedBy, @Comment);";

            var cmd = new CommandDefinition(
                sql,
                new
                {
                    OrderId = orderId,
                    StoreId = storeId,
                    ExpectedVer = expectedVersion,
                    NewStatus = newStatus,
                    ChangedBy = changedBy,
                    Comment = comment
                },
                Transaction,
                cancellationToken: ct);

            await Connection.ExecuteAsync(cmd);
        });
    }

    public async Task CreateOrderProcedureAsync(
        Guid orderId,
        Guid customerId,
        Guid storeId,
        string deliveryProvider,
        string city,
        string warehouseAddress,
        string recipientPhone,
        string? notes,
        string paymentMethod,
        List<OrderItem> items,
        CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);

            // Serialize order items into JSONB for usp_create_order stored procedure
            var itemsJson = JsonSerializer.Serialize(items.Select(x => new
            {
                product_id = x.ProductId,
                quantity = x.Quantity
            }));

            const string sql = @"
                CALL usp_create_order(
                    @OrderId, 
                    @CustomerId, 
                    @StoreId, 
                    @DeliveryProvider, 
                    @City, 
                    @WarehouseAddress, 
                    @RecipientPhone, 
                    @Notes, 
                    @ItemsJson::jsonb, 
                    @PaymentMethod);";

            var cmd = new CommandDefinition(
                sql,
                new
                {
                    OrderId = orderId,
                    CustomerId = customerId,
                    StoreId = storeId,
                    DeliveryProvider = deliveryProvider,
                    City = city,
                    WarehouseAddress = warehouseAddress,
                    RecipientPhone = recipientPhone,
                    Notes = notes,
                    ItemsJson = itemsJson,
                    PaymentMethod = paymentMethod
                },
                Transaction,
                cancellationToken: ct);

            await Connection.ExecuteAsync(cmd);
        });
    }
}

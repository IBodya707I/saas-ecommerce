using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Domain.Entities;

namespace Orders.Dal.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetWithItemsAsync(Guid orderId, Guid storeId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByStoreAndStatusAsync(Guid storeId, string status, CancellationToken ct = default);
    Task ChangeStatusAsync(Guid orderId, Guid storeId, int expectedVersion, string newStatus, string changedBy, string? comment = null, CancellationToken ct = default);
    Task CreateOrderProcedureAsync(Guid orderId, Guid customerId, Guid storeId, string deliveryProvider, string city, string warehouseAddress, string recipientPhone, string? notes, string paymentMethod, List<OrderItem> items, CancellationToken ct = default);
}

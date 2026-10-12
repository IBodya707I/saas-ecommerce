using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Bll.Dtos.Orders;

namespace Orders.Bll.Interfaces;

/// <summary>
/// Service contract for order management and transactional business operations.
/// </summary>
public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid orderId, Guid storeId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetByStoreAndStatusAsync(Guid storeId, string status, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task PayOrderAsync(Guid orderId, Guid storeId, CancellationToken ct = default);
    Task ChangeStatusAsync(Guid orderId, ChangeOrderStatusDto dto, CancellationToken ct = default);
}

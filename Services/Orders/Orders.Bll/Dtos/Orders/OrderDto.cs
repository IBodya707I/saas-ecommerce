using System;
using System.Collections.Generic;

namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object representing the full order aggregate in API responses.
/// </summary>
public record OrderDto(
    Guid Id,
    Guid StoreId,
    Guid CustomerId,
    string Status,
    string PaymentMethod,
    decimal TotalAmount,
    int Version,
    DateTime CreatedAt,
    OrderDetailsDto? Details,
    IReadOnlyList<OrderItemDto> Items);

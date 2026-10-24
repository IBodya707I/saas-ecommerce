using System;

namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object representing an order item in API responses.
/// </summary>
public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

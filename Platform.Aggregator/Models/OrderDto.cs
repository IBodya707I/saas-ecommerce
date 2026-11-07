namespace Platform.Aggregator.Models;

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

public record OrderDetailsDto(
    string ShippingAddress,
    string City,
    string PostalCode,
    string Country,
    string? RecipientName,
    string? RecipientPhone);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

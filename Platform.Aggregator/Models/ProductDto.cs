namespace Platform.Aggregator.Models;

public record ProductDto(
    Guid Id,
    Guid StoreId,
    Guid CategoryId,
    string Name,
    decimal Price,
    string Sku,
    int AvailableStock,
    bool IsActive);

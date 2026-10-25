namespace Catalog.Bll.Dtos;

public record ProductDetailDto(
    Guid ProductId,
    string Characteristics,
    string? Description);

public record ProductImageDto(
    Guid Id,
    string Url,
    int DisplayOrder);

public record ProductDto(
    Guid Id,
    Guid StoreId,
    Guid CategoryId,
    string Name,
    string Sku,
    decimal Price,
    int AvailableStock,
    bool IsActive,
    ProductDetailDto? Detail,
    IReadOnlyList<ProductImageDto> Images);

public record CreateProductDto(
    Guid StoreId,
    Guid CategoryId,
    string Name,
    string Sku,
    decimal Price,
    int AvailableStock = 0,
    string Characteristics = "{}",
    Dictionary<string, object>? Attributes = null,
    string? Description = null,
    List<string>? ImageUrls = null);

public record UpdateProductDto(
    string Name,
    string Sku,
    Guid CategoryId,
    decimal Price,
    int AvailableStock,
    string Characteristics = "{}",
    Dictionary<string, object>? Attributes = null,
    string? Description = null,
    bool IsActive = true);

public record UpdateProductPriceDto(
    decimal NewPrice);

public record AdjustStockDto(
    int Quantity);

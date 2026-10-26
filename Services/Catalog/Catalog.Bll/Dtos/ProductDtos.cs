using System.ComponentModel.DataAnnotations;

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
    [property: Required(ErrorMessage = "StoreId is required")]
    Guid StoreId,

    [property: Required(ErrorMessage = "CategoryId is required")]
    Guid CategoryId,

    [property: Required(ErrorMessage = "Product name is required")]
    [property: StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    string Name,

    [property: Required(ErrorMessage = "SKU is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "SKU must be between 2 and 100 characters")]
    [property: RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "SKU must contain only letters, numbers, hyphens or underscores")]
    string Sku,

    [property: Range(0.01, 100000000, ErrorMessage = "Price must be greater than 0")]
    decimal Price,

    [property: Range(0, int.MaxValue, ErrorMessage = "Available stock cannot be negative")]
    int AvailableStock = 0,

    string Characteristics = "{}",
    Dictionary<string, object>? Attributes = null,
    string? Description = null,
    List<string>? ImageUrls = null);

public record UpdateProductDto(
    [property: Required(ErrorMessage = "Product name is required")]
    [property: StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    string Name,

    [property: Required(ErrorMessage = "SKU is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "SKU must be between 2 and 100 characters")]
    [property: RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "SKU must contain only letters, numbers, hyphens or underscores")]
    string Sku,

    [property: Required(ErrorMessage = "CategoryId is required")]
    Guid CategoryId,

    [property: Range(0.01, 100000000, ErrorMessage = "Price must be greater than 0")]
    decimal Price,

    [property: Range(0, int.MaxValue, ErrorMessage = "Available stock cannot be negative")]
    int AvailableStock = 0,

    string Characteristics = "{}",
    Dictionary<string, object>? Attributes = null,
    string? Description = null,
    bool IsActive = true);

public record UpdateProductPriceDto(
    [property: Range(0.01, 100000000, ErrorMessage = "Price must be greater than 0")]
    decimal NewPrice);

public record AdjustStockDto(
    [property: Range(-100000, 100000, ErrorMessage = "Quantity delta must be within valid range")]
    int Quantity);

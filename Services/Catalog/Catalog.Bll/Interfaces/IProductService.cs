using Catalog.Bll.Dtos;

namespace Catalog.Bll.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<ProductDto> UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct = default);
    Task<ProductDto> AdjustStockAsync(Guid id, int quantity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

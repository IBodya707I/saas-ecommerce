using System.Text.Json;
using AutoMapper;
using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using Catalog.Bll.Specifications;
using Catalog.Dal;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Bll.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
    {
        _uow.SetStoreContext(query.StoreId);

        var countSpec = new ProductsFilterSpec(query, applyPagingAndSorting: false);
        var totalCount = await _uow.Products.CountAsync(countSpec, ct);

        var pagedSpec = new ProductsFilterSpec(query, applyPagingAndSorting: true);
        var products = await _uow.Products.ListAsync(pagedSpec, ct);

        var dtos = _mapper.Map<IReadOnlyList<ProductDto>>(products);
        return new PagedResult<ProductDto>(dtos, totalCount, query.Page, query.PageSize);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetWithDetailsAsync(id, ct);
        if (product == null)
            throw new NotFoundException($"Product with ID '{id}' was not found");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        _uow.SetStoreContext(dto.StoreId);

        if (await _uow.Products.ExistsBySkuAsync(dto.StoreId, dto.Sku, ct))
            throw new ConflictException($"Product with SKU '{dto.Sku}' already exists in this store");

        var productId = Guid.NewGuid();
        var product = new Product(
            productId,
            dto.StoreId,
            dto.CategoryId,
            dto.Name,
            dto.Sku,
            dto.Price,
            dto.AvailableStock
        );

        var normChars = NormalizeCharacteristics(dto.Characteristics, dto.Attributes);
        product.SetDetail(new ProductDetail(productId, normChars, dto.Description));

        if (dto.ImageUrls != null)
        {
            var order = 0;
            foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                product.Images.Add(new ProductImage(Guid.NewGuid(), productId, url.Trim(), order++));
            }
        }

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetWithDetailsAsync(id, ct);
        if (product == null)
            throw new NotFoundException($"Product with ID '{id}' was not found");

        _uow.SetStoreContext(product.StoreId);

        if (!string.Equals(product.Sku, dto.Sku, StringComparison.OrdinalIgnoreCase))
        {
            if (await _uow.Products.ExistsBySkuAsync(product.StoreId, dto.Sku, ct))
                throw new ConflictException($"Product with SKU '{dto.Sku}' already exists in this store");
        }

        product.UpdateDetails(dto.Name, dto.Sku, dto.CategoryId);
        product.UpdatePrice(dto.Price);

        if (dto.AvailableStock > product.AvailableStock)
            product.AddStock(dto.AvailableStock - product.AvailableStock);
        else if (dto.AvailableStock < product.AvailableStock)
            product.RemoveStock(product.AvailableStock - dto.AvailableStock);

        if (dto.IsActive && !product.IsActive)
            product.Activate();
        else if (!dto.IsActive && product.IsActive)
            product.Deactivate();

        var normChars = NormalizeCharacteristics(dto.Characteristics, dto.Attributes);
        if (product.Detail == null)
        {
            product.SetDetail(new ProductDetail(product.Id, normChars, dto.Description));
        }
        else
        {
            product.Detail.UpdateCharacteristics(normChars);
            product.Detail.UpdateDescription(dto.Description);
        }

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdatePriceAsync(Guid id, decimal newPrice, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetWithDetailsAsync(id, ct);
        if (product == null)
            throw new NotFoundException($"Product with ID '{id}' was not found");

        _uow.SetStoreContext(product.StoreId);
        product.UpdatePrice(newPrice);

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> AdjustStockAsync(Guid id, int quantity, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetWithDetailsAsync(id, ct);
        if (product == null)
            throw new NotFoundException($"Product with ID '{id}' was not found");

        _uow.SetStoreContext(product.StoreId);

        if (quantity > 0)
            product.AddStock(quantity);
        else if (quantity < 0)
            product.RemoveStock(-quantity);

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product == null)
            throw new NotFoundException($"Product with ID '{id}' was not found");

        _uow.SetStoreContext(product.StoreId);
        product.MarkDeleted();

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);
    }

    private static string NormalizeCharacteristics(string characteristics, Dictionary<string, object>? attributes)
    {
        if (attributes != null && attributes.Count > 0)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in attributes)
            {
                var key = k.Trim().ToLowerInvariant();
                if (v is string s)
                    dict[key] = s.Trim();
                else
                    dict[key] = v;
            }
            return JsonSerializer.Serialize(dict);
        }

        if (!string.IsNullOrWhiteSpace(characteristics) && characteristics != "{}")
        {
            try
            {
                using var doc = JsonDocument.Parse(characteristics);
                var dict = new Dictionary<string, object>();
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var key = prop.Name.Trim().ToLowerInvariant();
                    dict[key] = prop.Value.Clone();
                }
                return JsonSerializer.Serialize(dict);
            }
            catch
            {
                return "{}";
            }
        }

        return "{}";
    }
}

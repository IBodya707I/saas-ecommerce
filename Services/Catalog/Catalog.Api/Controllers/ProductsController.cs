using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Catalog.Domain.Exceptions.ValidationException;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged([FromQuery] ProductQuery query, CancellationToken ct)
    {
        if (query.StoreId == Guid.Empty &&
            Request.Headers.TryGetValue("X-Store-Id", out var headerVal) &&
            Guid.TryParse(headerVal, out var headerStoreId))
        {
            query = query with { StoreId = headerStoreId };
        }

        if (query.StoreId == Guid.Empty)
        {
            throw new ValidationException("StoreId is required either as a query parameter or 'X-Store-Id' header");
        }

        var result = await _productService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException("Product validation failed", errors);
        }

        var created = await _productService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException("Product update validation failed", errors);
        }

        var updated = await _productService.UpdateAsync(id, dto, ct);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/price")]
    public async Task<ActionResult<ProductDto>> UpdatePrice(Guid id, [FromBody] UpdateProductPriceDto dto, CancellationToken ct)
    {
        var updated = await _productService.UpdatePriceAsync(id, dto.NewPrice, ct);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<ActionResult<ProductDto>> AdjustStock(Guid id, [FromBody] AdjustStockDto dto, CancellationToken ct)
    {
        var updated = await _productService.AdjustStockAsync(id, dto.Quantity, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return NoContent();
    }
}

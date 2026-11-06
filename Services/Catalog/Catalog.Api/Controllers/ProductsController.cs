using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValidationException = Catalog.Domain.Exceptions.ValidationException;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("Catalog.Api");
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
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

        using var activity = ActivitySource.StartActivity("CreateProduct");
        var created = await _productService.CreateAsync(dto, ct);
        activity?.SetTag("product.id", created.Id);

        _logger.LogInformation("Product {ProductId} created with name '{ProductName}' and price {Price}",
            created.Id, created.Name, created.Price);

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

        using var activity = ActivitySource.StartActivity("UpdateProduct");
        activity?.SetTag("product.id", id);

        var updated = await _productService.UpdateAsync(id, dto, ct);

        _logger.LogInformation("Product {ProductId} updated successfully", id);

        return Ok(updated);
    }

    [HttpPatch("{id:guid}/price")]
    public async Task<ActionResult<ProductDto>> UpdatePrice(Guid id, [FromBody] UpdateProductPriceDto dto, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("UpdateProductPrice");
        activity?.SetTag("product.id", id);

        var updated = await _productService.UpdatePriceAsync(id, dto.NewPrice, ct);

        _logger.LogInformation("Product {ProductId} price updated to {Price}", id, dto.NewPrice);

        return Ok(updated);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<ActionResult<ProductDto>> AdjustStock(Guid id, [FromBody] AdjustStockDto dto, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("AdjustProductStock");
        activity?.SetTag("product.id", id);

        var updated = await _productService.AdjustStockAsync(id, dto.Quantity, ct);

        _logger.LogInformation("Product {ProductId} stock adjusted by {Quantity}", id, dto.Quantity);

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return NoContent();
    }
}

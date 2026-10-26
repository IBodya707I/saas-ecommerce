using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Catalog.Domain.Exceptions.ValidationException;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;

    public CategoriesController(
        ICategoryService categoryService,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator)
    {
        _categoryService = categoryService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll([FromQuery] Guid? storeId, CancellationToken ct)
    {
        var resolvedStoreId = ResolveStoreId(storeId);
        var categories = await _categoryService.GetAllAsync(resolvedStoreId, ct);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdWithSubCategoriesAsync(id, ct);
        return Ok(category);
    }

    [HttpGet("{id:guid}/filters")]
    public async Task<ActionResult<IReadOnlyList<CategoryFilterDto>>> GetFilters(
        Guid id,
        [FromQuery] Guid? storeId,
        CancellationToken ct)
    {
        var resolvedStoreId = ResolveStoreId(storeId);
        var filters = await _categoryService.GetCategoryFiltersAsync(resolvedStoreId, id, ct);
        return Ok(filters);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException("Category validation failed", errors);
        }

        var created = await _categoryService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException("Category update validation failed", errors);
        }

        var updated = await _categoryService.UpdateAsync(id, dto, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        return NoContent();
    }

    private Guid ResolveStoreId(Guid? storeId)
    {
        if (storeId.HasValue && storeId.Value != Guid.Empty)
            return storeId.Value;

        if (Request.Headers.TryGetValue("X-Store-Id", out var headerVal) &&
            Guid.TryParse(headerVal, out var headerStoreId) &&
            headerStoreId != Guid.Empty)
        {
            return headerStoreId;
        }

        throw new ValidationException("StoreId is required either as a query parameter or 'X-Store-Id' header");
    }
}

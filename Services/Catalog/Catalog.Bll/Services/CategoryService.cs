using System.Text.Json;
using AutoMapper;
using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using Catalog.Bll.Specifications;
using Catalog.Dal;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Bll.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid storeId, CancellationToken ct = default)
    {
        _uow.SetStoreContext(storeId);
        var categories = await _uow.Categories.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories.Where(c => !c.IsDeleted));
    }

    public async Task<CategoryDto> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _uow.Categories.GetWithSubCategoriesExplicitAsync(id, ct);
        if (category == null)
            throw new NotFoundException($"Category with ID '{id}' was not found");

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<IReadOnlyList<CategoryFilterDto>> GetCategoryFiltersAsync(
        Guid storeId,
        Guid categoryId,
        CancellationToken ct = default)
    {
        _uow.SetStoreContext(storeId);

        var products = await _uow.Products.ListAsync(new ActiveProductsByCategorySpec(categoryId), ct);

        var rawOccurrences = new List<(string Key, string Value)>();

        foreach (var product in products)
        {
            if (product.Detail == null || string.IsNullOrWhiteSpace(product.Detail.Characteristics))
                continue;

            try
            {
                using var doc = JsonDocument.Parse(product.Detail.Characteristics);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var key = prop.Name.Trim().ToLowerInvariant();
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            var val = item.ToString().Trim();
                            if (!string.IsNullOrEmpty(val))
                                rawOccurrences.Add((key, val));
                        }
                    }
                    else
                    {
                        var val = prop.Value.ToString().Trim();
                        if (!string.IsNullOrEmpty(val))
                            rawOccurrences.Add((key, val));
                    }
                }
            }
            catch
            {
            }
        }

        var filterGroups = rawOccurrences
            .GroupBy(x => x.Key)
            .Select(group => new CategoryFilterDto(
                Key: group.Key,
                Title: char.ToUpper(group.Key[0]) + group.Key[1..].Replace('_', ' '),
                Type: "checkbox",
                Options: group
                    .GroupBy(x => x.Value)
                    .Select(opt => new FilterOptionDto(opt.Key, opt.Count()))
                    .OrderByDescending(opt => opt.Count)
                    .ToList()
            ))
            .OrderBy(f => f.Key)
            .ToList();

        return filterGroups;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        _uow.SetStoreContext(dto.StoreId);

        var existing = await _uow.Categories.GetAllAsync(ct);
        if (existing.Any(c => string.Equals(c.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
            throw new ConflictException($"Category with slug '{dto.Slug}' already exists in this store");

        var category = new Category(
            Guid.NewGuid(),
            dto.StoreId,
            dto.Name,
            dto.Slug,
            dto.ParentCategoryId
        );

        await _uow.Categories.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var category = await _uow.Categories.GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
            throw new NotFoundException($"Category with ID '{id}' was not found");

        _uow.SetStoreContext(category.StoreId);

        if (!string.Equals(category.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _uow.Categories.GetAllAsync(ct);
            if (existing.Any(c => c.Id != id && string.Equals(c.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
                throw new ConflictException($"Category with slug '{dto.Slug}' already exists in this store");
        }

        category.Update(dto.Name, dto.Slug, "system");
        category.SetParentCategory(dto.ParentCategoryId, "system");

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _uow.Categories.GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
            throw new NotFoundException($"Category with ID '{id}' was not found");

        _uow.SetStoreContext(category.StoreId);
        category.MarkDeleted("system");

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(ct);
    }
}

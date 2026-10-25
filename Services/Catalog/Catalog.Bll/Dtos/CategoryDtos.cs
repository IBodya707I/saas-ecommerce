namespace Catalog.Bll.Dtos;

public record FilterOptionDto(string Value, int Count);

public record CategoryFilterDto(
    string Key,
    string Title,
    string Type,
    IReadOnlyList<FilterOptionDto> Options);

public record CategoryDto(
    Guid Id,
    Guid StoreId,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    IReadOnlyList<CategoryDto>? SubCategories = null);

public record CreateCategoryDto(
    Guid StoreId,
    string Name,
    string Slug,
    Guid? ParentCategoryId = null);

public record UpdateCategoryDto(
    string Name,
    string Slug,
    Guid? ParentCategoryId = null);

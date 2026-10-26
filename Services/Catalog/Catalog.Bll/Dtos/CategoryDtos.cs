using System.ComponentModel.DataAnnotations;

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
    [property: Required(ErrorMessage = "StoreId is required")]
    Guid StoreId,

    [property: Required(ErrorMessage = "Category name is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    string Name,

    [property: Required(ErrorMessage = "Category slug is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Category slug must be between 2 and 100 characters")]
    [property: RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Category slug must be lowercase alphanumeric with hyphens")]
    string Slug,

    Guid? ParentCategoryId = null);

public record UpdateCategoryDto(
    [property: Required(ErrorMessage = "Category name is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    string Name,

    [property: Required(ErrorMessage = "Category slug is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Category slug must be between 2 and 100 characters")]
    [property: RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Category slug must be lowercase alphanumeric with hyphens")]
    string Slug,

    Guid? ParentCategoryId = null);

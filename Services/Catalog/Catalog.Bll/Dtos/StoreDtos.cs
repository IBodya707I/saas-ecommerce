using System.ComponentModel.DataAnnotations;

namespace Catalog.Bll.Dtos;

public record StoreDto(
    Guid Id,
    string Name,
    string Slug,
    Guid OwnerId,
    string Category);

public record CreateStoreDto(
    [property: Required(ErrorMessage = "Store name is required")]
    [property: StringLength(150, MinimumLength = 2, ErrorMessage = "Store name must be between 2 and 150 characters")]
    string Name,

    [property: Required(ErrorMessage = "Store slug is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "Store slug must be between 2 and 100 characters")]
    [property: RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Store slug must be lowercase alphanumeric with hyphens")]
    string Slug,

    [property: Required(ErrorMessage = "OwnerId is required")]
    Guid OwnerId,

    [property: Required(ErrorMessage = "Category is required")]
    [property: StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    string Category);

public record UpdateStoreDto(
    [property: Required(ErrorMessage = "Store name is required")]
    [property: StringLength(150, MinimumLength = 2, ErrorMessage = "Store name must be between 2 and 150 characters")]
    string Name,

    [property: Required(ErrorMessage = "Category is required")]
    [property: StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    string Category);

public record StoreEmployeeDto(
    Guid StoreId,
    Guid UserId,
    string Role,
    DateTime AssignedAt,
    bool IsActive,
    string? UserFullName = null,
    string? UserEmail = null);

public record AssignStoreEmployeeDto(
    [property: Required(ErrorMessage = "StoreId is required")]
    Guid StoreId,

    [property: Required(ErrorMessage = "UserId is required")]
    Guid UserId,

    [property: Required(ErrorMessage = "Role is required")]
    [property: RegularExpression(@"^(Owner|Manager|CatalogEditor|OrderOperator)$", ErrorMessage = "Invalid role")]
    string Role = "Manager");

public record UpdateStoreEmployeeRoleDto(
    [property: Required(ErrorMessage = "NewRole is required")]
    [property: RegularExpression(@"^(Owner|Manager|CatalogEditor|OrderOperator)$", ErrorMessage = "Invalid role")]
    string NewRole);

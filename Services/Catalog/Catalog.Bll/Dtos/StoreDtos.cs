namespace Catalog.Bll.Dtos;

public record StoreDto(
    Guid Id,
    string Name,
    string Slug,
    Guid OwnerId,
    string Category);

public record CreateStoreDto(
    string Name,
    string Slug,
    Guid OwnerId,
    string Category);

public record UpdateStoreDto(
    string Name,
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
    Guid StoreId,
    Guid UserId,
    string Role = "Manager");

public record UpdateStoreEmployeeRoleDto(
    string NewRole);

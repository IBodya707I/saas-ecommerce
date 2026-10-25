namespace Catalog.Bll.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string SystemRole,
    DateTime CreatedAt);

public record CreateUserDto(
    string Email,
    string Password,
    string FullName,
    string? Phone = null,
    string SystemRole = "Merchant");

public record UpdateUserProfileDto(
    string FullName,
    string? Phone);

public record ChangeUserRoleDto(
    string NewRole);

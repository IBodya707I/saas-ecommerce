using System;

namespace Orders.Bll.Dtos.Customers;

/// <summary>
/// Data transfer object representing a customer in API responses.
/// </summary>
public record CustomerDto(
    Guid Id,
    Guid StoreId,
    string Phone,
    string? Email,
    string FullName,
    bool IsRegistered,
    int Version,
    DateTime CreatedAt);

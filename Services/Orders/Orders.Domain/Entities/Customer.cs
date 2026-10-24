using System;

namespace Orders.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsRegistered { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

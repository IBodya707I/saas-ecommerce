using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class StoreEmployee
{
    public Guid StoreId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = "Manager";
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public Store? Store { get; private set; }
    public User? User { get; private set; }

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Owner", "Manager", "CatalogEditor", "OrderOperator"
    };

    private StoreEmployee() { } 

    public StoreEmployee(
        Guid storeId,
        Guid userId,
        string role = "Manager",
        DateTime? assignedAt = null,
        string createdBy = "system")
    {
        if (storeId == Guid.Empty)
            throw new DomainException("StoreId cannot be empty");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        if (string.IsNullOrWhiteSpace(role) || !ValidRoles.Contains(role.Trim()))
            throw new DomainException($"Invalid employee role: {role}. Valid roles are: Owner, Manager, CatalogEditor, OrderOperator");

        StoreId = storeId;
        UserId = userId;
        Role = role.Trim();
        AssignedAt = assignedAt ?? DateTime.UtcNow;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void ChangeRole(string newRole, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newRole) || !ValidRoles.Contains(newRole.Trim()))
            throw new DomainException($"Invalid employee role: {newRole}");

        Role = newRole.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void MarkDeleted(string updatedBy)
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

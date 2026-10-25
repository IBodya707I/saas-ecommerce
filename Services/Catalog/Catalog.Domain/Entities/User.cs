using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string SystemRole { get; private set; } = "Merchant";
    public int Version { get; private set; } = 1;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public ICollection<Store> OwnedStores { get; private set; } = new List<Store>();
    public ICollection<StoreEmployee> StoreEmployees { get; private set; } = new List<StoreEmployee>();

    private static readonly HashSet<string> ValidSystemRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "SuperAdmin", "PlatformAdmin", "Merchant", "Staff"
    };

    private User() { } 

    public User(
        Guid id,
        string email,
        string passwordHash,
        string fullName,
        string? phone = null,
        string systemRole = "Merchant",
        string createdBy = "system")
    {
        if (id == Guid.Empty)
            throw new DomainException("User Id cannot be empty");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        if (!ValidSystemRoles.Contains(systemRole.Trim()))
            throw new DomainException($"Invalid system role: {systemRole}. Allowed roles: SuperAdmin, PlatformAdmin, Merchant, Staff");

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        Phone = phone?.Trim();
        SystemRole = systemRole.Trim();
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdateProfile(string fullName, string? phone, string updatedBy = "system")
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty");

        FullName = fullName.Trim();
        Phone = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void ChangeRole(string role, string updatedBy = "system")
    {
        if (!ValidSystemRoles.Contains(role.Trim()))
            throw new DomainException($"Invalid system role: {role}");

        SystemRole = role.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void ChangePassword(string newPasswordHash, string updatedBy = "system")
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void MarkDeleted(string updatedBy = "system")
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }
}

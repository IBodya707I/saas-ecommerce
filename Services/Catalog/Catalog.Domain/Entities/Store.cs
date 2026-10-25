using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class Store
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public User? Owner { get; private set; }
    public ICollection<Category> Categories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<StoreEmployee> StoreEmployees { get; private set; } = new List<StoreEmployee>();

    private Store() { } 

    public Store(
        Guid id,
        string name,
        string slug,
        Guid ownerId,
        string category,
        string createdBy = "system")
    {
        if (id == Guid.Empty)
            throw new DomainException("Store Id cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Store name is required");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Store slug is required");

        if (ownerId == Guid.Empty)
            throw new DomainException("Store OwnerId cannot be empty");

        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Store category is required");

        Id = id;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        OwnerId = ownerId;
        Category = category.Trim();
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdateDetails(string name, string category, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Store name cannot be empty");

        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Store category cannot be empty");

        Name = name.Trim();
        Category = category.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void ChangeSlug(string newSlug, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newSlug))
            throw new DomainException("Store slug cannot be empty");

        Slug = newSlug.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void MarkDeleted(string updatedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }
}

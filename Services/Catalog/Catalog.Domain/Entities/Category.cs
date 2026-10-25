using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public Store? Store { get; private set; }
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { } 

    public Category(
        Guid id,
        Guid storeId,
        string name,
        string slug,
        Guid? parentCategoryId = null,
        string createdBy = "system")
    {
        if (id == Guid.Empty)
            throw new DomainException("Category Id cannot be empty");

        if (storeId == Guid.Empty)
            throw new DomainException("Store Id cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Category slug is required");

        if (parentCategoryId.HasValue && parentCategoryId.Value == id)
            throw new DomainException("Category cannot be its own parent");

        Id = id;
        StoreId = storeId;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentCategoryId = parentCategoryId;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void Update(string name, string slug, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Category slug cannot be empty");

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void SetParentCategory(Guid? parentCategoryId, string updatedBy)
    {
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
            throw new DomainException("Category cannot be its own parent");

        ParentCategoryId = parentCategoryId;
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

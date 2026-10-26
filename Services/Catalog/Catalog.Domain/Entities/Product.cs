using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int AvailableStock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; } = 1;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public Store? Store { get; private set; }
    public Category? Category { get; private set; }
    public ProductDetail? Detail { get; private set; }
    public ProductDetail? Details => Detail;
    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();

    private Product() { } 

    public Product(
        Guid id,
        Guid storeId,
        Guid categoryId,
        string name,
        string sku,
        decimal price,
        int availableStock = 0,
        string createdBy = "system")
    {
        if (id == Guid.Empty)
            throw new DomainException("Product Id cannot be empty");

        if (storeId == Guid.Empty)
            throw new DomainException("Store Id cannot be empty");

        if (categoryId == Guid.Empty)
            throw new DomainException("Category Id cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU is required");

        if (price <= 0)
            throw new DomainException("Price must be positive");

        if (availableStock < 0)
            throw new DomainException("Available stock cannot be negative");

        Id = id;
        StoreId = storeId;
        CategoryId = categoryId;
        Name = name.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        Price = price;
        AvailableStock = availableStock;
        IsActive = true;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdatePrice(decimal newPrice, string updatedBy = "system")
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be positive");

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void AddStock(int quantity, string updatedBy = "system")
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        AvailableStock += quantity;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void RemoveStock(int quantity, string updatedBy = "system")
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        if (AvailableStock < quantity)
            throw new DomainException($"Only {AvailableStock} units available");

        AvailableStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void UpdateDetails(string name, string sku, Guid categoryId, string updatedBy = "system")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU cannot be empty");

        if (categoryId == Guid.Empty)
            throw new DomainException("Category Id cannot be empty");

        Name = name.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void SetDetail(ProductDetail detail)
    {
        Detail = detail ?? throw new DomainException("Product detail cannot be null");
    }

    public void Activate(string updatedBy = "system")
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void Deactivate(string updatedBy = "system")
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void MarkDeleted(string updatedBy = "system")
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }
}

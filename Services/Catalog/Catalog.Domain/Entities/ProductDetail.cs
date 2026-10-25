using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class ProductDetail
{
    public Guid ProductId { get; private set; }
    public string Characteristics { get; private set; } = "{}";
    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public Product? Product { get; private set; }

    private ProductDetail() { } 

    public ProductDetail(
        Guid productId,
        string characteristics = "{}",
        string? description = null,
        string createdBy = "system")
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty");

        ProductId = productId;
        Characteristics = string.IsNullOrWhiteSpace(characteristics) ? "{}" : characteristics.Trim();
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdateCharacteristics(string characteristics, string updatedBy)
    {
        Characteristics = string.IsNullOrWhiteSpace(characteristics) ? "{}" : characteristics.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateDescription(string? description, string updatedBy)
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

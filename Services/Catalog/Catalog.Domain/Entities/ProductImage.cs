using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public Product? Product { get; private set; }

    private ProductImage() { } 

    public ProductImage(
        Guid id,
        Guid productId,
        string url,
        int displayOrder = 0,
        string createdBy = "system")
    {
        if (id == Guid.Empty)
            throw new DomainException("ProductImage Id cannot be empty");

        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty");

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL is required");

        if (displayOrder < 0)
            throw new DomainException("Display order must be greater than or equal to 0");

        Id = id;
        ProductId = productId;
        Url = url.Trim();
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdateDisplayOrder(int displayOrder, string updatedBy)
    {
        if (displayOrder < 0)
            throw new DomainException("Display order must be greater than or equal to 0");

        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateUrl(string url, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL cannot be empty");

        Url = url.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

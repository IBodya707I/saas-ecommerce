using System.Text.Json;
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
        Characteristics = ValidateAndNormalizeJson(characteristics);
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    public void UpdateCharacteristics(string characteristics, string updatedBy = "system")
    {
        Characteristics = ValidateAndNormalizeJson(characteristics);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void UpdateDescription(string? description, string updatedBy = "system")
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    private static string ValidateAndNormalizeJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            using var doc = JsonDocument.Parse(json);
            return json.Trim();
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Characteristics must be a valid JSON string: {ex.Message}", ex);
        }
    }
}

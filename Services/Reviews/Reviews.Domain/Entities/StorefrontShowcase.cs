namespace Reviews.Domain.Entities;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;
using Reviews.Domain.ValueObjects;

public class StorefrontShowcase : BaseEntity
{
    public string StoreId { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string StoreName { get; private set; } = string.Empty;
    public Branding Branding { get; private set; } = null!;
    public string? WorkSchedule { get; private set; }

    private StorefrontShowcase() { }

    public StorefrontShowcase(string storeId, string slug, string storeName, Branding branding, string? workSchedule = null)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            throw new DomainException("StoreId is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug is required.");

        if (string.IsNullOrWhiteSpace(storeName))
            throw new DomainException("StoreName is required.");

        if (branding is null)
            throw new DomainException("Branding is required.");

        StoreId = storeId.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        StoreName = storeName.Trim();
        Branding = branding;
        WorkSchedule = string.IsNullOrWhiteSpace(workSchedule) ? null : workSchedule.Trim();
    }

    public void UpdateBranding(Branding branding)
    {
        Branding = branding ?? throw new DomainException("Branding cannot be null.");
        Touch();
    }

    public void UpdateSchedule(string? workSchedule)
    {
        WorkSchedule = string.IsNullOrWhiteSpace(workSchedule) ? null : workSchedule.Trim();
        Touch();
    }

    public void UpdateInfo(string storeName, string slug)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new DomainException("StoreName is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug is required.");

        StoreName = storeName.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Touch();
    }
}

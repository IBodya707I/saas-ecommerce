namespace Reviews.Application.Common.Models;

public record StorefrontDto(
    string Id,
    string StoreId,
    string Slug,
    string StoreName,
    string Color,
    string FontFamily,
    string? LogoUrl,
    string? WorkSchedule);

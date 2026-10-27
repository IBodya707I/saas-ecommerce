namespace Reviews.Domain.ValueObjects;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;

public sealed class Branding : ValueObject
{
    public string Color { get; }
    public string FontFamily { get; }
    public string? LogoUrl { get; }

    public Branding(string color, string fontFamily, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Branding color is required.");

        if (string.IsNullOrWhiteSpace(fontFamily))
            throw new DomainException("Branding fontFamily is required.");

        Color = color.Trim();
        FontFamily = fontFamily.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Color;
        yield return FontFamily;
        yield return LogoUrl;
    }
}

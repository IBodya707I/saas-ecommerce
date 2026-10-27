namespace Reviews.Application.Storefronts.Commands.UpdateStorefrontBranding;

using Reviews.Application.Common.Interfaces;

public record UpdateStorefrontBrandingCommand(
    string StoreId,
    string Color,
    string FontFamily,
    string? LogoUrl = null) : ICommand;

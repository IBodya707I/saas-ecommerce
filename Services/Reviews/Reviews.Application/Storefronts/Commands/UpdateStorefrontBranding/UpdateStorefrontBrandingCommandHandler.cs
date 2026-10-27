namespace Reviews.Application.Storefronts.Commands.UpdateStorefrontBranding;

using MediatR;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Domain.ValueObjects;

public class UpdateStorefrontBrandingCommandHandler : IRequestHandler<UpdateStorefrontBrandingCommand>
{
    private readonly IStorefrontRepository _repository;

    public UpdateStorefrontBrandingCommandHandler(IStorefrontRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateStorefrontBrandingCommand cmd, CancellationToken ct)
    {
        var storefront = await _repository.GetByStoreIdAsync(cmd.StoreId, ct)
            ?? throw new NotFoundException(nameof(StorefrontShowcase), cmd.StoreId);

        var branding = new Branding(cmd.Color, cmd.FontFamily, cmd.LogoUrl);
        storefront.UpdateBranding(branding);

        await _repository.UpdateAsync(storefront, ct);
    }
}

namespace Reviews.WebApi.Controllers;

using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reviews.Application.Common.Models;
using Reviews.Application.Storefronts.Commands.UpdateStorefrontBranding;
using Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

[ApiController]
[Route("api/[controller]")]
public class StorefrontsController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("Reviews.WebApi");
    private readonly ISender _sender;
    private readonly ILogger<StorefrontsController> _logger;

    public StorefrontsController(ISender sender, ILogger<StorefrontsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(StorefrontDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StorefrontDto>> GetBySlug(string slug, CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("GetStorefrontBySlug");
        activity?.SetTag("storefront.slug", slug);

        var result = await _sender.Send(new GetStorefrontBySlugQuery(slug), ct);
        return Ok(result);
    }

    [HttpPut("{storeId}/branding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBranding(
        string storeId,
        [FromBody] UpdateBrandingRequest request,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateStorefrontBranding");
        activity?.SetTag("storefront.storeId", storeId);

        await _sender.Send(new UpdateStorefrontBrandingCommand(storeId, request.Color, request.FontFamily, request.LogoUrl), ct);

        _logger.LogInformation("Storefront {StoreId} branding updated with color {Color} and font {FontFamily}",
            storeId, request.Color, request.FontFamily);

        return NoContent();
    }
}

public record UpdateBrandingRequest(string Color, string FontFamily, string? LogoUrl);

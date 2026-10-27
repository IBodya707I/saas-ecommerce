namespace Reviews.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reviews.Application.Common.Models;
using Reviews.Application.Storefronts.Commands.UpdateStorefrontBranding;
using Reviews.Application.Storefronts.Queries.GetStorefrontBySlug;

[ApiController]
[Route("api/[controller]")]
public class StorefrontsController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(StorefrontDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StorefrontDto>> GetBySlug(string slug, CancellationToken ct = default)
    {
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
        await _sender.Send(new UpdateStorefrontBrandingCommand(storeId, request.Color, request.FontFamily, request.LogoUrl), ct);
        return NoContent();
    }
}

public record UpdateBrandingRequest(string Color, string FontFamily, string? LogoUrl);

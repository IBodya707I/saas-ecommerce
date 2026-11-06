namespace Reviews.WebApi.Controllers;

using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reviews.Application.Commands.CreateReview;
using Reviews.Application.Commands.DeleteReview;
using Reviews.Application.Commands.UpdateReview;
using Reviews.Application.Common.Models;
using Reviews.Application.Queries.GetReviewById;
using Reviews.Application.Queries.GetReviewsList;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("Reviews.WebApi");
    private readonly ISender _sender;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(ISender sender, ILogger<ReviewsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetList(
        [FromQuery] long? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetReviewsListQuery(productId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> GetById(string id, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetReviewByIdQuery(id), ct);
        Response.Headers.ETag = $"\"{result.Version}\"";
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<string>> Create(
        [FromBody] CreateReviewCommand command,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("CreateReview");
        var id = await _sender.Send(command, ct);
        activity?.SetTag("review.id", id);
        activity?.SetTag("review.productId", command.ProductId);

        _logger.LogInformation("Review {ReviewId} created for product {ProductId} by user {UserId} with rating {Rating}",
            id, command.ProductId, command.AuthorId, command.Rating);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateReviewRequest request,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateReview");
        activity?.SetTag("review.id", id);

        await _sender.Send(new UpdateReviewCommand(id, request.Rating, request.Text), ct);

        _logger.LogInformation("Review {ReviewId} updated with rating {Rating}", id, request.Rating);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("DeleteReview");
        activity?.SetTag("review.id", id);

        await _sender.Send(new DeleteReviewCommand(id), ct);

        _logger.LogInformation("Review {ReviewId} deleted", id);

        return NoContent();
    }
}

public record UpdateReviewRequest(int Rating, string Text);

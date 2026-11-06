namespace Reviews.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ISender _sender;

    public ReviewsController(ISender sender)
    {
        _sender = sender;
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
        var id = await _sender.Send(command, ct);
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
        await _sender.Send(new UpdateReviewCommand(id, request.Rating, request.Text), ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        await _sender.Send(new DeleteReviewCommand(id), ct);
        return NoContent();
    }
}

public record UpdateReviewRequest(int Rating, string Text);

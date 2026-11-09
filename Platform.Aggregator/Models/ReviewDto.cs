namespace Platform.Aggregator.Models;

public record ReviewDto(
    string Id,
    long ProductId,
    string ProductName,
    string AuthorName,
    int Rating,
    string Text,
    int CommentsCount,
    int Version,
    DateTime CreatedAt);

public record ReviewsPagedResult(
    IReadOnlyList<ReviewDto> Items,
    long TotalCount,
    int Page,
    int PageSize);

namespace Reviews.Application.Common.Models;

public record ReviewDto(
    string Id,
    long ProductId,
    string ProductName,
    string AuthorName,
    int Rating,
    string Text,
    int CommentsCount,
    int Version,
    DateTime CreatedAt = default);

namespace Reviews.Application.Commands.CreateReview;

using Reviews.Application.Common.Interfaces;

public record CreateReviewCommand(
    long ProductId,
    string ProductName,
    string AuthorId,
    string AuthorName,
    int Rating,
    string Text) : ICommand<string>;

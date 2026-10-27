namespace Reviews.Application.Commands.DeleteReview;

using Reviews.Application.Common.Interfaces;

public record DeleteReviewCommand(string Id) : ICommand;

namespace Reviews.Application.Commands.UpdateReview;

using Reviews.Application.Common.Interfaces;

public record UpdateReviewCommand(string Id, int Rating, string Text) : ICommand;

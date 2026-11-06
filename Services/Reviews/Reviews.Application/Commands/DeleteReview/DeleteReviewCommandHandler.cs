namespace Reviews.Application.Commands.DeleteReview;

using MediatR;
using MongoDB.Bson;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _repository;

    public DeleteReviewCommandHandler(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteReviewCommand cmd, CancellationToken ct)
    {
        if (!ObjectId.TryParse(cmd.Id, out var objectId))
            throw new NotFoundException(nameof(Review), cmd.Id);

        var review = await _repository.GetByIdAsync(objectId, ct)
            ?? throw new NotFoundException(nameof(Review), cmd.Id);

        await _repository.DeleteAsync(objectId, ct);
    }
}

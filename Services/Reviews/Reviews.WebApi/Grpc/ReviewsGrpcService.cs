namespace Reviews.WebApi.Grpc;

using System.Threading.Tasks;
using FluentValidation;
using global::Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Reviews.Application.Common.Exceptions;
using Reviews.Application.Common.Models;
using Reviews.Application.Queries.GetReviewById;
using Reviews.Application.Queries.GetReviewsList;

public class ReviewsGrpcService : ReviewsService.ReviewsServiceBase
{
    private readonly ISender _sender;
    private readonly ILogger<ReviewsGrpcService> _logger;

    public ReviewsGrpcService(ISender sender, ILogger<ReviewsGrpcService> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public override async Task<ReviewsReply> GetReviewsByProductId(GetReviewsByProductIdRequest request, ServerCallContext context)
    {
        if (request.ProductId <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProductId must be greater than zero"));
        }

        int page = request.Page > 0 ? request.Page : 1;
        int pageSize = request.PageSize > 0 ? request.PageSize : 20;

        try
        {
            var pagedResult = await _sender.Send(new GetReviewsListQuery(request.ProductId, page, pageSize), context.CancellationToken);
            var reply = new ReviewsReply();
            foreach (var item in pagedResult.Items)
            {
                reply.Items.Add(MapToReply(item));
            }
            return reply;
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<ReviewReply> GetReviewById(GetReviewByIdRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Review Id must not be empty"));
        }

        try
        {
            var review = await _sender.Send(new GetReviewByIdQuery(request.Id), context.CancellationToken);
            return MapToReply(review);
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    private static ReviewReply MapToReply(ReviewDto r)
    {
        return new ReviewReply
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.ProductName,
            AuthorName = r.AuthorName,
            Rating = r.Rating,
            Text = r.Text,
            CommentsCount = r.CommentsCount,
            Version = r.Version,
            CreatedAt = r.CreatedAt.ToString("O")
        };
    }
}

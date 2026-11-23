namespace Catalog.Api.Grpc;

using System;
using System.Threading.Tasks;
using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using Catalog.Domain.Exceptions;
using global::Grpc.Core;
using Microsoft.Extensions.Logging;

public class CatalogGrpcService : CatalogService.CatalogServiceBase
{
    private readonly IProductService _productService;
    private readonly ILogger<CatalogGrpcService> _logger;

    public CatalogGrpcService(IProductService productService, ILogger<CatalogGrpcService> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || !Guid.TryParse(request.Id, out var productId) || productId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid product Guid ID must be provided"));
        }

        try
        {
            var product = await _productService.GetByIdAsync(productId, context.CancellationToken);
            return MapToReply(product);
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

    public override async Task<ProductsReply> GetProductsByIds(GetProductsByIdsRequest request, ServerCallContext context)
    {
        var reply = new ProductsReply();
        foreach (var idStr in request.Ids)
        {
            if (Guid.TryParse(idStr, out var id) && id != Guid.Empty)
            {
                try
                {
                    var product = await _productService.GetByIdAsync(id, context.CancellationToken);
                    reply.Products.Add(MapToReply(product));
                }
                catch (NotFoundException)
                {
                    _logger.LogWarning("Product with id {ProductId} not found during batch gRPC query", id);
                }
            }
        }

        return reply;
    }

    private static ProductReply MapToReply(ProductDto p)
    {
        return new ProductReply
        {
            Id = p.Id.ToString(),
            StoreId = p.StoreId.ToString(),
            CategoryId = p.CategoryId.ToString(),
            Name = p.Name,
            Sku = p.Sku,
            Price = (double)p.Price,
            AvailableStock = p.AvailableStock,
            IsActive = p.IsActive,
            Description = p.Detail?.Description ?? string.Empty
        };
    }
}

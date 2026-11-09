namespace Orders.Api.Grpc;

using System;
using System.Threading.Tasks;
using global::Grpc.Core;
using Microsoft.Extensions.Logging;
using Orders.Bll.Dtos.Orders;
using Orders.Bll.Interfaces;
using Orders.Domain.Exceptions;

public class OrdersGrpcService : OrdersService.OrdersServiceBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersGrpcService> _logger;

    public OrdersGrpcService(IOrderService orderService, ILogger<OrdersGrpcService> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public override async Task<OrderReply> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || !Guid.TryParse(request.Id, out var orderId) || orderId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid order Guid ID must be provided"));
        }

        if (string.IsNullOrWhiteSpace(request.StoreId) || !Guid.TryParse(request.StoreId, out var storeId) || storeId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid store Guid ID must be provided"));
        }

        try
        {
            var order = await _orderService.GetByIdAsync(orderId, storeId, context.CancellationToken);
            return MapToReply(order);
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

    private static OrderReply MapToReply(OrderDto o)
    {
        var reply = new OrderReply
        {
            Id = o.Id.ToString(),
            StoreId = o.StoreId.ToString(),
            CustomerId = o.CustomerId.ToString(),
            Status = o.Status,
            PaymentMethod = o.PaymentMethod,
            TotalAmount = (double)o.TotalAmount,
            Version = o.Version,
            CreatedAt = o.CreatedAt.ToString("O")
        };

        if (o.Details != null)
        {
            reply.Details = new OrderDetailsReply
            {
                DeliveryProvider = o.Details.DeliveryProvider,
                City = o.Details.City,
                WarehouseAddress = o.Details.WarehouseAddress,
                RecipientPhone = o.Details.RecipientPhone,
                Notes = o.Details.Notes ?? string.Empty
            };
        }

        foreach (var item in o.Items)
        {
            reply.Items.Add(new OrderItemReply
            {
                ProductId = item.ProductId.ToString(),
                ProductName = item.ProductName,
                UnitPrice = (double)item.UnitPrice,
                Quantity = item.Quantity
            });
        }

        return reply;
    }
}

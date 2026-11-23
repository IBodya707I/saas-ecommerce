using System.Text.Json;
using System.Text.Json.Serialization;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Platform.Aggregator.Clients;
using Platform.ServiceDefaults;
using Polly;
using Serilog;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("redis", configureOptions: options =>
{
    options.ConnectTimeout = 1000;
    options.AbortOnConnectFail = false;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var criticalGrpcRetryPolicy = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 4,
        InitialBackoff = TimeSpan.FromSeconds(0.5),
        MaxBackoff = TimeSpan.FromSeconds(4),
        BackoffMultiplier = 2,
        RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.ResourceExhausted }
    }
};

var nonCriticalGrpcRetryPolicy = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 2,
        InitialBackoff = TimeSpan.FromSeconds(0.2),
        MaxBackoff = TimeSpan.FromSeconds(1),
        BackoffMultiplier = 1.5,
        RetryableStatusCodes = { StatusCode.Unavailable }
    }
};

builder.Services.AddGrpcClient<Orders.Api.Grpc.OrdersService.OrdersServiceClient>(o =>
{
    o.Address = new Uri("https://orders-api");
})
.ConfigureChannel(channel => channel.ServiceConfig = new ServiceConfig
{
    MethodConfigs = { criticalGrpcRetryPolicy }
});

builder.Services.AddGrpcClient<Catalog.Api.Grpc.CatalogService.CatalogServiceClient>(o =>
{
    o.Address = new Uri("https://catalog-api");
})
.ConfigureChannel(channel => channel.ServiceConfig = new ServiceConfig
{
    MethodConfigs = { criticalGrpcRetryPolicy }
});

builder.Services.AddGrpcClient<Reviews.WebApi.Grpc.ReviewsService.ReviewsServiceClient>(o =>
{
    o.Address = new Uri("https://reviews-api");
})
.ConfigureChannel(channel => channel.ServiceConfig = new ServiceConfig
{
    MethodConfigs = { nonCriticalGrpcRetryPolicy }
});

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("https://catalog-api");
})
.RemoveAllResilienceHandlers()
.AddStandardResilienceHandler(o =>
{
    o.Retry.MaxRetryAttempts = 5;
    o.Retry.Delay = TimeSpan.FromSeconds(1);
    o.Retry.BackoffType = DelayBackoffType.Exponential;
    o.Retry.UseJitter = true;
    o.Retry.DisableForUnsafeHttpMethods();

    o.CircuitBreaker.FailureRatio = 0.3; 
    o.CircuitBreaker.MinimumThroughput = 10;
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    o.CircuitBreaker.OnOpened = args =>
    {
        Log.Error("Circuit opened for CatalogClient for {Duration}", args.BreakDuration);
        return ValueTask.CompletedTask;
    };

    o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<OrdersClient>(client =>
{
    client.BaseAddress = new Uri("https://orders-api");
})
.RemoveAllResilienceHandlers()
.AddStandardResilienceHandler(o =>
{
    o.Retry.MaxRetryAttempts = 5;
    o.Retry.Delay = TimeSpan.FromSeconds(1);
    o.Retry.BackoffType = DelayBackoffType.Exponential;
    o.Retry.UseJitter = true;
    o.Retry.DisableForUnsafeHttpMethods();

    o.CircuitBreaker.FailureRatio = 0.3;
    o.CircuitBreaker.MinimumThroughput = 10;
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    o.CircuitBreaker.OnOpened = args =>
    {
        Log.Error("Circuit opened for OrdersClient for {Duration}", args.BreakDuration);
        return ValueTask.CompletedTask;
    };

    o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<ReviewsClient>(client =>
{
    client.BaseAddress = new Uri("https://reviews-api");
})
.RemoveAllResilienceHandlers()
.AddStandardResilienceHandler(o =>
{
    o.Retry.MaxRetryAttempts = 1; 
    o.Retry.Delay = TimeSpan.FromSeconds(0.5);
    o.Retry.BackoffType = DelayBackoffType.Exponential;
    o.Retry.UseJitter = true;
    o.Retry.DisableForUnsafeHttpMethods();

    o.CircuitBreaker.FailureRatio = 0.7; 
    o.CircuitBreaker.MinimumThroughput = 10;
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);
    o.CircuitBreaker.OnOpened = args =>
    {
        Log.Warning("Circuit opened for ReviewsClient for {Duration}", args.BreakDuration);
        return ValueTask.CompletedTask;
    };

    o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2); 
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

using System.Text.Json;
using System.Text.Json.Serialization;
using Platform.Aggregator.Clients;
using Platform.ServiceDefaults;

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

builder.Services.AddGrpcClient<Orders.Api.Grpc.OrdersService.OrdersServiceClient>(o =>
{
    o.Address = new Uri("https://orders-api");
});

builder.Services.AddGrpcClient<Catalog.Api.Grpc.CatalogService.CatalogServiceClient>(o =>
{
    o.Address = new Uri("https://catalog-api");
});

builder.Services.AddGrpcClient<Reviews.WebApi.Grpc.ReviewsService.ReviewsServiceClient>(o =>
{
    o.Address = new Uri("https://reviews-api");
});

builder.Services.AddHttpClient<OrdersClient>(client =>
{
    client.BaseAddress = new Uri("http://orders-api");
});

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://catalog-api");
});

builder.Services.AddHttpClient<ReviewsClient>(client =>
{
    client.BaseAddress = new Uri("http://reviews-api");
});

var app = builder.Build();

// 4. Default Endpoints (/health, /alive)
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

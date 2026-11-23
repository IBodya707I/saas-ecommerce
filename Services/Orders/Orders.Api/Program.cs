using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orders.Api.Middlewares;
using Orders.Bll.Interfaces;
using Orders.Bll.Mapping;
using Orders.Bll.Services;
using Orders.Dal.Interfaces;
using Orders.Dal.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. ServiceDefaults: Serilog, OpenTelemetry, HealthChecks, Service Discovery, Resilience
builder.AddServiceDefaults();

// Configure Kestrel to support HTTP/1.1 (REST) and HTTP/2 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<Platform.ServiceDefaults.Grpc.LoggingInterceptor>();
});

builder.AddRedisDistributedCache("redis", configureOptions: options =>
{
    options.ConnectTimeout = 1000;
    options.AbortOnConnectFail = false;
});
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10_000;
    options.CompactionPercentage = 0.05;
});

// 2. Database Connection String with Aspire and environment variable override support
var connectionString = builder.Configuration.GetConnectionString("OrdersDb")
    ?? builder.Configuration.GetConnectionString("orders-db")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing");

// 3. Dependency Injection: Scoped Unit of Work (one connection & transaction per HTTP request)
builder.Services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(connectionString));

// 4. BLL Services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 5. AutoMapper Profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<OrderMappingProfile>();
});

// 6. Controllers & JSON Configuration
builder.Services.AddControllers();

// 7. Centralized ProblemDetails & Domain Exception Handler
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// 8. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ServiceDefaults Endpoints: HealthChecks (/health, /alive) та CorrelationId Middleware
app.MapDefaultEndpoints();

// Pipeline middleware configuration
app.UseExceptionHandler();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<Orders.Api.Grpc.OrdersGrpcService>();

app.Run();

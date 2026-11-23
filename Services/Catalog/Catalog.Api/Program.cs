using Catalog.Api.Middlewares;
using Catalog.Bll.Interfaces;
using Catalog.Bll.Mapping;
using Catalog.Bll.Services;
using Catalog.Bll.Validators;
using Catalog.Dal;
using Catalog.Dal.Repositories;
using Catalog.Dal.Seeding;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddSingleton<Catalog.Bll.Caching.CacheMetrics>();
builder.Services.AddScoped<Catalog.Bll.Caching.ITwoLevelCacheService, Catalog.Bll.Caching.TwoLevelCacheService>();

// 2. Database Connection String with Aspire and environment variable override support
var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? builder.Configuration.GetConnectionString("catalog-db")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=5432;Database=catalog_db;Username=postgres;Password=postgres;";

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddDbContext<CatalogDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
    });
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStoreEmployeeRepository, StoreEmployeeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoreService, StoreService>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<CatalogMappingProfile>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ServiceDefaults Endpoints: HealthChecks (/health, /alive) та CorrelationId Middleware
app.MapDefaultEndpoints();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        if (!app.Environment.IsProduction())
        {
            await DbInitializer.SeedAsync(db);
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not apply migrations or seed on startup: {Message}", ex.Message);
    }
}

app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<Catalog.Api.Grpc.CatalogGrpcService>();

app.Run();

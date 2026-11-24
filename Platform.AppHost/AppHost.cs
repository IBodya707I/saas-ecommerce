var builder = DistributedApplication.CreateBuilder(args);

var pgPassword = builder.AddParameter("postgres-password", "{N3sSr5x!V)}xAEEh4Y60k", secret: true);
var postgres = builder.AddPostgres("postgres", password:pgPassword)
    .WithDataVolume();

var ordersDb = postgres.AddDatabase("OrdersDb");
var catalogDb = postgres.AddDatabase("CatalogDb");

var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume();

var showcaseDb = mongo.AddDatabase("showcase-db");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var orders = builder.AddProject<Projects.Orders_Api>("orders-api")
    .WithReference(ordersDb)
    .WaitFor(ordersDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

var catalog = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

var reviews = builder.AddProject<Projects.Reviews_WebApi>("reviews-api")
    .WithReference(showcaseDb)
    .WaitFor(showcaseDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

var aggregator = builder.AddProject<Projects.Platform_Aggregator>("aggregator")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(orders)
    .WaitFor(orders)
    .WithReference(catalog)
    .WaitFor(catalog)
    .WithReference(reviews)
    .WaitFor(reviews)
    .WithHttpHealthCheck("/health");

var gateway = builder.AddProject<Projects.Platform_ApiGateway>("gateway")
    .WithHttpEndpoint(port: 5000, name: "public")
    .WithReference(orders)
    .WithReference(catalog)
    .WithReference(reviews)
    .WithReference(aggregator)
    .WaitFor(aggregator)
    .WithHttpHealthCheck("/health");

builder.Build().Run();

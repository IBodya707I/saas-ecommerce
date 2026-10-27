var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var ordersDb = postgres.AddDatabase("OrdersDb");
var catalogDb = postgres.AddDatabase("CatalogDb");

var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume();

var showcaseDb = mongo.AddDatabase("showcase-db");

builder.AddProject<Projects.Orders_Api>("orders-api")
    .WithReference(ordersDb)
    .WaitFor(ordersDb);

builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

builder.AddProject<Projects.Reviews_WebApi>("reviews-api")
    .WithReference(showcaseDb)
    .WaitFor(showcaseDb);

builder.Build().Run();

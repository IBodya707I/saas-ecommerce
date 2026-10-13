var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("OrdersDb");

//var postgres = builder.AddConnectionString("OrdersDb");

builder.AddProject<Projects.Orders_Api>("orders-api")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();

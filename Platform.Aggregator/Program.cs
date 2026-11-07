using System.Text.Json;
using System.Text.Json.Serialization;
using Platform.Aggregator.Clients;
using Platform.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// 1. Aspire Service Defaults (Serilog, OpenTelemetry, HealthChecks, Service Discovery, Resilience)
builder.AddServiceDefaults();

// 2. Configure Controllers with camelCase and ignore nulls (Criterion 14)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Register Typed HttpClients with Aspire Service Discovery (Criterion 11)
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

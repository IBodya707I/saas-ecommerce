using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());


var connectionString = builder.Configuration.GetConnectionString("OrdersDb")
    ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing");


//builder.Services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(connectionString));
//builder.Services.AddScoped<IOrderService, OrderService>();


builder.Services.AddControllers();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});
//builder.Services.AddExceptionHandler<DomainExceptionHandler>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
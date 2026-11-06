using Reviews.Application;
using Reviews.Application.Common.Mappings;
using Reviews.Infrastructure;
using Reviews.Infrastructure.Seeding;
using Reviews.WebApi.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Reviews.WebApi application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddMaps(typeof(ReviewProfile).Assembly);
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Seeding was skipped or deferred (MongoDB might not be running yet).");
        }
    }

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

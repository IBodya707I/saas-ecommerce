using System.Diagnostics;
using Platform.ServiceDefaults;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();

    string? route = null;
    try
    {
        route = context.GetReverseProxyFeature()?.Route?.Config?.RouteId;
    }
    catch (InvalidOperationException)
    {
    }

    if (route != null)
    {
        app.Logger.LogInformation(
            "{Method} {Path} -> route {Route}: {StatusCode} in {Elapsed} ms",
            context.Request.Method,
            context.Request.Path,
            route,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
});

app.MapReverseProxy();

app.MapDefaultEndpoints();

app.Run();

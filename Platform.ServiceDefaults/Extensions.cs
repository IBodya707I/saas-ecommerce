namespace Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Platform.ServiceDefaults;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // 1. Serilog - структуроване логування у JSON з експортом у OpenTelemetry для Aspire Dashboard
        builder.Services.AddSerilog((services, cfg) => cfg
            .MinimumLevel.Is(builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("StackExchange.Redis", LogEventLevel.Fatal)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(new CompactJsonFormatter())
            , writeToProviders: true);

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdHandler>();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
            http.AddHttpMessageHandler<CorrelationIdHandler>();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Catalog.Cache")
                    .AddMeter("Polly")
                    .AddMeter("Orders.Api")
                    .AddMeter("Catalog.Api");
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource("Platform.Aggregator")
                    .AddSource("Orders.Api")
                    .AddSource("Catalog.Api")
                    .AddSource("Reviews.WebApi")
                    .AddSource("Platform.Messaging")
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource("Npgsql")
                    .AddSource("Microsoft.EntityFrameworkCore")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse
        });

        return app;
    }

    private static async Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new System.Text.Json.JsonWriterOptions { Indented = true };
        using var memoryStream = new System.IO.MemoryStream();
        await using (var writer = new System.Text.Json.Utf8JsonWriter(memoryStream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", report.Status.ToString());
            writer.WriteString("totalDuration", report.TotalDuration.ToString());
            writer.WriteStartObject("entries");

            foreach (var entry in report.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteString("description", entry.Value.Description);
                writer.WriteString("duration", entry.Value.Duration.ToString());
                if (entry.Value.Exception != null)
                {
                    writer.WriteString("error", entry.Value.Exception.Message);
                }
                writer.WriteStartArray("tags");
                foreach (var tag in entry.Value.Tags)
                {
                    writer.WriteStringValue(tag);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        await context.Response.Body.WriteAsync(memoryStream.ToArray());
    }
}

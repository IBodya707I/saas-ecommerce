namespace Platform.ServiceDefaults;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private const string AppliedKey = "__CorrelationIdApplied";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Items.ContainsKey(AppliedKey))
        {
            await _next(context);
            return;
        }
        context.Items[AppliedKey] = true;

        if (!context.Request.Headers.TryGetValue(HeaderName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            value = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = value;
        }

        context.Response.Headers[HeaderName] = value;

        using (LogContext.PushProperty("CorrelationId", value.ToString()))
        {
            _logger.LogInformation("Request started {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
            _logger.LogInformation("Request finished with {StatusCode}", context.Response.StatusCode);
        }
    }
}

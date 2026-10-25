using Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Middlewares;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;
    private readonly IConfiguration _configuration;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
            ConflictException => (StatusCodes.Status409Conflict, "Business Conflict"),
            DomainException => (StatusCodes.Status409Conflict, "Domain Rule Violation"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled error: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Domain exception [{StatusCode}]: {Message}", statusCode, exception.Message);
        }

        var isDev = _configuration.GetValue<bool>("IsDevelopment", true);
        var detail = isDev
            ? (exception.InnerException?.Message ?? exception.Message)
            : (statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred" : exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException valEx && valEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = valEx.Errors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

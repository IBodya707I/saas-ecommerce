namespace Reviews.WebApi.Infrastructure;

using System.Net.Mime;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reviews.Application.Common.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        int statusCode;
        string title;
        string detail = exception.Message;
        IDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = StatusCodes.Status404NotFound;
                title = "Resource Not Found";
                detail = notFoundEx.Message;
                break;

            case ValidationException validationEx:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation Failed";
                detail = "One or more validation errors occurred.";
                errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case ConflictException conflictEx:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict";
                detail = conflictEx.Message;
                break;

            case MongoWriteException mongoWriteEx when mongoWriteEx.WriteError?.Category == ServerErrorCategory.DuplicateKey:
                statusCode = StatusCodes.Status409Conflict;
                title = "Duplicate Key Conflict";
                detail = "A resource with these details already exists.";
                break;

            case MongoWriteException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Database Write Error";
                detail = "A database write error occurred.";
                break;

            case MongoConnectionException or TimeoutException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                title = "Service Unavailable";
                detail = "Unable to connect to the database service.";
                break;

            case { } ex when ex.GetType().Name == "DomainException":
                statusCode = StatusCodes.Status400BadRequest;
                title = "Domain Rule Violation";
                detail = ex.Message;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "Internal Server Error";
                detail = "An unexpected error occurred.";
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        ProblemDetails problemDetails;
        if (errors != null)
        {
            problemDetails = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

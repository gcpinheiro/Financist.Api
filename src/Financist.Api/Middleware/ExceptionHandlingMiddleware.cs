using Financist.Application.Common.Exceptions;
using Financist.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, extensions, logLevel) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationException.Message,
                new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                },
                LogLevel.Warning),
            UnauthorizedException unauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorizedException.Message,
                new Dictionary<string, object?>(),
                LogLevel.Warning),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictException.Message,
                new Dictionary<string, object?>(),
                LogLevel.Information),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFoundException.Message,
                new Dictionary<string, object?>(),
                LogLevel.Information),
            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                domainException.Message,
                new Dictionary<string, object?>(),
                LogLevel.Warning),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "An unexpected error occurred while processing the request.",
                new Dictionary<string, object?>(),
                LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "Request failed with status code {StatusCode}", statusCode);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        foreach (var extension in extensions)
        {
            problemDetails.Extensions[extension.Key] = extension.Value;
        }

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

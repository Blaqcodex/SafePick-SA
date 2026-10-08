using System.Text.Json;
using SafePick.Application.Common.Exceptions;
using SafePick.Domain.Common;

namespace SafePick.Api.Middleware;

/// <summary>
/// Catches exceptions and translates them into consistent HTTP responses.
///
/// Without this middleware, an unhandled DomainException would surface
/// as a 500 Internal Server Error with a stack trace. That's correct
/// behaviour in development, but it's not what a domain rule violation
/// should look like: the client sent a request that violates a business
/// rule, which is a 400-class error.
///
/// This middleware maps:
///   - DomainException          → 400 Bad Request (a rule was violated)
///   - NotFoundException        → 404 Not Found
///   - ForbiddenAccessException → 403 Forbidden
///   - ValidationException      → 400 Bad Request (with field-level errors)
///   - everything else          → 500 Internal Server Error
///
/// The response body is a standard RFC 7807 problem-details document.
/// </summary>
public sealed partial class ExceptionHandlingMiddleware
{
    // Cached JsonSerializerOptions — reused for every response.
    // CA1869 requires this; constructing a new options object per
    // serialization is wasteful and unnecessary.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (DomainException ex)
        {
            // Convert PathString → string once, at the call site, so the
            // logging argument is not evaluated needlessly. CA1873.
            LogDomainViolation(_logger, ex,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty);

            await WriteProblemAsync(
                context,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Domain rule violation",
                detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            LogNotFound(_logger, ex,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty);

            await WriteProblemAsync(
                context,
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            LogForbidden(_logger, ex,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty);

            await WriteProblemAsync(
                context,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
        catch (ValidationException ex)
        {
            LogValidationFailed(_logger, ex,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty);

            await WriteValidationProblemAsync(context, ex.Errors);
        }
        catch (Exception ex)
        {
            LogUnhandled(_logger, ex,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty);

            await WriteProblemAsync(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred",
                detail: "The request could not be completed. Please try again later.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://tools.ietf.org/html/rfc9110#section-15.{statusCode / 100}",
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, JsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static async Task WriteValidationProblemAsync(
        HttpContext context,
        IDictionary<string, string[]> errors)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = StatusCodes.Status400BadRequest,
            errors,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, JsonOptions);

        await context.Response.WriteAsync(json);
    }

    // ---------------------------------------------------------------
    // Source-generated logging (CA1848 / CA1873-compliant).
    //
    // Partial methods have no accessibility modifier — C# treats them
    // as private, and the LoggerMessage source generator engages
    // reliably with this signature.
    // ---------------------------------------------------------------

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Domain rule violation on {Method} {Path}")]
    static partial void LogDomainViolation(
        ILogger logger, Exception ex, string method, string path);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Resource not found on {Method} {Path}")]
    static partial void LogNotFound(
        ILogger logger, Exception ex, string method, string path);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Forbidden access on {Method} {Path}")]
    static partial void LogForbidden(
        ILogger logger, Exception ex, string method, string path);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Validation failed on {Method} {Path}")]
    static partial void LogValidationFailed(
        ILogger logger, Exception ex, string method, string path);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unhandled exception on {Method} {Path}")]
    static partial void LogUnhandled(
        ILogger logger, Exception ex, string method, string path);
}
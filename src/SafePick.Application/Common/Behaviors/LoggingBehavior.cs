using MediatR;
using Microsoft.Extensions.Logging;

namespace SafePick.Application.Common.Behaviors;

/// <summary>
/// Logs the start and end of every request handled by MediatR.
///
/// Uses LoggerMessage source-generated delegates (CA1848-compliant) so
/// logging is allocation-free when the log level is disabled.
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        LogHandling(_logger, requestName);

        var response = await next();

        LogHandled(_logger, requestName);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName}")]
    private static partial void LogHandled(ILogger logger, string requestName);
}
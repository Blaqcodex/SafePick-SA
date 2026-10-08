using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SafePick.Application.Common.Events;
using SafePick.Domain.Common;

namespace SafePick.Infrastructure.Persistence.Outbox;

/// <summary>
/// The background dispatcher for the transactional outbox.
///
/// It periodically polls the outbox_messages table for undispatched
/// messages, reconstructs the domain event from its stored payload,
/// and publishes it via MediatR. On success, the message is marked
/// dispatched. On failure, the attempt count is incremented and the
/// error is recorded for later inspection.
/// </summary>
public sealed partial class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;
    private const int MaxAttempts = 10;

    public OutboxDispatcher(
        IServiceProvider serviceProvider,
        ILogger<OutboxDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogDispatcherStarted(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogDispatchIterationFailed(_logger, ex);
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var now = DateTime.UtcNow;

        var pending = await context.OutboxMessages
            .Where(m => m.DispatchedAt == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        LogDispatchingPending(_logger, pending.Count);

        foreach (var message in pending)
        {
            try
            {
                var eventType = Type.GetType(message.EventType)
                    ?? throw new InvalidOperationException(
                        $"Cannot resolve event type '{message.EventType}'.");

                var domainEvent = JsonSerializer.Deserialize(
                    message.Payload, eventType) as IDomainEvent
                    ?? throw new InvalidOperationException(
                        "Deserialized payload is not an IDomainEvent.");

                var notificationType = typeof(DomainEventNotification<>)
                    .MakeGenericType(eventType);
                var notification = Activator.CreateInstance(
                    notificationType, domainEvent)
                    ?? throw new InvalidOperationException(
                        "Failed to construct notification wrapper.");

                await publisher.Publish(notification, cancellationToken);

                message.MarkDispatched(now);
            }
            catch (Exception ex)
            {
                LogDispatchFailed(_logger, ex, message.Id);
                message.RecordFailure(ex.Message);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------
    // Source-generated logging (CA1848-compliant).
    //
    // The partial methods have no accessibility modifier — C# treats
    // them as private, and the LoggerMessage source generator engages
    // reliably with this signature.
    // ---------------------------------------------------------------

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "OutboxDispatcher started.")]
    static partial void LogDispatcherStarted(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Outbox dispatch iteration failed.")]
    static partial void LogDispatchIterationFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Outbox: dispatching {Count} pending message(s).")]
    static partial void LogDispatchingPending(ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Outbox dispatch failed for message {MessageId}.")]
    static partial void LogDispatchFailed(
        ILogger logger, Exception ex, Guid messageId);
}
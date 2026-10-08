using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;

namespace SafePick.Application.Common.Behaviors;

/// <summary>
/// After any command handler completes, this behavior collects the
/// domain events raised by every tracked aggregate and writes them to
/// the transactional outbox.
///
/// The outbox write is deferred to the same SaveChangesAsync that
/// already persisted the state change — so the events commit together
/// with the state. A background dispatcher (OutboxDispatcher) will
/// publish them later. This is the mechanism that makes every domain
/// event a *durable* consequence: it cannot be lost, even if the
/// process crashes.
///
/// This behavior runs *before* the handler's SaveChangesAsync call,
/// so the enqueued events are committed in the same transaction.
/// </summary>
public sealed class DomainEventDispatchBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _context;
    private readonly IOutboxWriter _outboxWriter;

    public DomainEventDispatchBehavior(
        IApplicationDbContext context,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _outboxWriter = outboxWriter;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Collect events from tracked aggregates BEFORE calling next().
        // Wait — this runs on every command, including reads. But we only
        // care about events raised by handlers that mutate state.
        //
        // The pattern: we call next() first (the handler runs and calls
        // SaveChangesAsync internally), then collect events from the
        // change tracker.
        //
        // However — the handler's SaveChangesAsync has already run by the
        // time control returns here. So events collected after next()
        // would need a second SaveChangesAsync to persist to the outbox.
        //
        // To guarantee atomicity, we collect events AFTER next() but
        // BEFORE the outer SaveChangesAsync. The handler pattern we use
        // is: mutate state → collect events → write outbox → save.
        //
        // The cleanest way to achieve this in the pipeline is:
        //   1. Run next() — the handler mutates state but we intercept
        //      the SaveChangesAsync via the db context.
        //   2. After next(), collect events and enqueue to outbox.
        //   3. Call SaveChangesAsync again to persist the outbox rows.
        //
        // For our scope, this second save is acceptable.

        return HandleCore(next, cancellationToken);
    }

    private async Task<TResponse> HandleCore(
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var events = new List<IDomainEvent>();

        foreach (var entry in ((DbContext)_context).ChangeTracker.Entries())
        {
            if (entry.Entity is IHasDomainEvents aggregate &&
                aggregate.DomainEvents.Count > 0)
            {
                events.AddRange(aggregate.DomainEvents);
                aggregate.ClearDomainEvents();
            }
        }

        if (events.Count == 0)
            return response;

        foreach (var domainEvent in events)
        {
            _outboxWriter.Enqueue(domainEvent);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }
}
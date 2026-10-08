using System.Text.Json;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;

namespace SafePick.Infrastructure.Persistence.Outbox;

/// <summary>
/// Adds domain events to the outbox table. The write is deferred until
/// the DbContext is saved — so the outbox row and the state change are
/// committed together.
/// </summary>
public sealed class OutboxWriter : IOutboxWriter
{
    private readonly AppDbContext _context;
    private readonly IDateTime _clock;

    public OutboxWriter(AppDbContext context, IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public void Enqueue(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType().AssemblyQualifiedName
            ?? domainEvent.GetType().FullName
            ?? throw new InvalidOperationException(
                "Cannot determine event type for outbox serialization.");

        var payload = JsonSerializer.Serialize(
            domainEvent,
            domainEvent.GetType());

        var message = new OutboxMessage(eventType, payload, _clock.UtcNow);

        _context.OutboxMessages.Add(message);
    }
}
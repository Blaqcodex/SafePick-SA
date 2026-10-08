namespace SafePick.Infrastructure.Persistence.Outbox;

/// <summary>
/// A durable record of a domain event that must eventually be dispatched.
///
/// Domain events are written here in the same transaction as the state
/// change that raised them. A background service reads undispatched
/// messages and publishes them via MediatR. This guarantees at-least-once
/// delivery of every domain event, without a message broker.
///
/// The combination of:
///   - atomic write (event + outbox row in one transaction)
///   - durable storage (the outbox is a table)
///   - a background dispatcher (polls and publishes)
///   - retry tracking (Attempts, LastError)
///
/// ...produces a guarantee: every domain event will eventually be
/// dispatched, even if the process crashes at any point.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    /// <summary>
    /// The fully-qualified CLR type name of the domain event, so the
    /// dispatcher can reconstruct it.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// The serialized domain event payload (JSON).
    /// </summary>
    public string Payload { get; private set; }

    /// <summary>
    /// When the event was written to the outbox.
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// When the event was successfully dispatched. Null until dispatched.
    /// </summary>
    public DateTime? DispatchedAt { get; private set; }

    /// <summary>
    /// How many times dispatch has been attempted.
    /// </summary>
    public int Attempts { get; private set; }

    /// <summary>
    /// The last error, if any dispatch attempt failed.
    /// </summary>
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public OutboxMessage(
        string eventType,
        string payload,
        DateTime occurredAt)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("EventType is required.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        Id = Guid.NewGuid();
        EventType = eventType;
        Payload = payload;
        OccurredAt = occurredAt;
        Attempts = 0;
    }

    public void MarkDispatched(DateTime now)
    {
        DispatchedAt = now;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error;
    }
}
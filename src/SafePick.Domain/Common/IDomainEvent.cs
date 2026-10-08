namespace SafePick.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// A domain event is an immutable statement that something meaningful
/// happened in the domain, expressed in the ubiquitous language.
///
/// Domain events are raised by aggregate roots and dispatched after the
/// aggregate's state has been persisted — never before.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The moment the event occurred, in server-authoritative time.
    /// </summary>
    DateTime OccurredOn { get; }
}
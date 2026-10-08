namespace SafePick.Domain.Common;

/// <summary>
/// Base class for aggregate roots.
///
/// An aggregate is a cluster of entities and value objects treated as a
/// single unit for the purpose of consistency. The aggregate root is the
/// only member the outside world may reference directly.
///
/// The aggregate root is also the only place from which domain events are
/// raised. Events are collected here and dispatched by the infrastructure
/// after the aggregate has been persisted.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// The domain events raised by this aggregate but not yet dispatched.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event. Called by aggregate methods when a
    /// meaningful state change occurs.
    /// </summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears the recorded events. Called by the infrastructure after the
    /// events have been dispatched, so they are not dispatched twice.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
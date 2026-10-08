namespace SafePick.Domain.Common;

/// <summary>
/// Implemented by every aggregate root. Allows infrastructure code to
/// access the aggregate's pending domain events without reflection.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
using SafePick.Domain.Common;

namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// Writes domain events to the transactional outbox. The write happens
/// in the same transaction as the state change that raised the event,
/// so the event is durably recorded if and only if the state is.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Adds the domain event to the current unit of work's outbox.
    /// The caller must have already tracked the state change; the
    /// outbox row will be committed together with it.
    /// </summary>
    void Enqueue(IDomainEvent domainEvent);
}
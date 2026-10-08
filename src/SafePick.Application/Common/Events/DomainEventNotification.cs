using MediatR;
using SafePick.Domain.Common;

namespace SafePick.Application.Common.Events;

/// <summary>
/// Bridges a domain event to MediatR's notification pipeline.
///
/// The Domain does not know about MediatR — its events implement
/// IDomainEvent only. This wrapper, defined in the Application layer,
/// carries a domain event as a MediatR INotification so it can be
/// dispatched through the pipeline.
///
/// This is the standard "domain event notification" pattern: the
/// Domain stays pure, and the Application layer adapts.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
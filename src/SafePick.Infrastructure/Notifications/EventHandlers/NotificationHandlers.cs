using MediatR;
using SafePick.Application.Common.Events;
using SafePick.Domain.Custody.Events;
using SafePick.Infrastructure.Notifications;

namespace SafePick.Application.Custody.EventHandlers;

/// <summary>
/// Notification handlers translate domain events into notification calls.
///
/// Each handler is thin: it receives the event, extracts the information
/// needed to notify, and calls the dispatcher. The dispatcher does the
/// work of resolving recipients and building the message.
///
/// The handlers are registered automatically via MediatR's assembly scan.
/// </summary>
public sealed class LearnerPickedUpNotificationHandler
    : INotificationHandler<DomainEventNotification<LearnerPickedUp>>
{
    private readonly NotificationDispatcher _dispatcher;

    public LearnerPickedUpNotificationHandler(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(
        DomainEventNotification<LearnerPickedUp> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        await _dispatcher.SendPickupConfirmationAsync(
            schoolId: evt.SchoolId,
            learnerId: evt.LearnerId,
            custodyEventId: evt.CustodyEventId,
            cancellationToken: cancellationToken);
    }
}

public sealed class LearnerNotInCarNotificationHandler
    : INotificationHandler<DomainEventNotification<LearnerNotInCar>>
{
    private readonly NotificationDispatcher _dispatcher;

    public LearnerNotInCarNotificationHandler(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(
        DomainEventNotification<LearnerNotInCar> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        await _dispatcher.SendNotInCarAsync(
            schoolId: evt.SchoolId,
            learnerId: evt.LearnerId,
            reason: evt.Reason,
            cancellationToken: cancellationToken);
    }
}

public sealed class CustodyGapOpenedNotificationHandler
    : INotificationHandler<DomainEventNotification<CustodyGapOpened>>
{
    private readonly NotificationDispatcher _dispatcher;

    public CustodyGapOpenedNotificationHandler(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(
        DomainEventNotification<CustodyGapOpened> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        await _dispatcher.SendCustodyGapAlertAsync(
            schoolId: evt.SchoolId,
            learnerId: evt.LearnerId,
            cancellationToken: cancellationToken);
    }
}
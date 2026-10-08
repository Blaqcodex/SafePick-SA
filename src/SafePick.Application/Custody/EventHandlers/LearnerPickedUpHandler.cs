using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Events;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;
using SafePick.Domain.Custody.Events;

namespace SafePick.Application.Custody.EventHandlers;

/// <summary>
/// Handles the LearnerPickedUp domain event.
///
/// If an open custody gap exists for this learner on the same school
/// day, this handler resolves it automatically — closing the loop
/// between reconciliation (which opens gaps) and custody recording
/// (which produces the resolving events).
/// </summary>
public sealed class LearnerPickedUpHandler
    : INotificationHandler<DomainEventNotification<LearnerPickedUp>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public LearnerPickedUpHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(
        DomainEventNotification<LearnerPickedUp> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var windowStart = domainEvent.ServerTimestamp.Date;
        var windowEnd = windowStart.AddDays(1);

        var openGap = await _context.CustodyGaps
            .Where(g => g.LearnerId == domainEvent.LearnerId &&
                        g.SchoolId == domainEvent.SchoolId &&
                        g.State == GapState.Open &&
                        g.ExpectedWindowStart >= windowStart &&
                        g.ExpectedWindowStart < windowEnd)
            .FirstOrDefaultAsync(cancellationToken);

        if (openGap is null)
            return;

        openGap.ResolveByCustodyEvent(domainEvent.CustodyEventId, _clock.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
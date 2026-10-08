using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Events;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;
using SafePick.Domain.Custody.Events;

namespace SafePick.Application.Custody.EventHandlers;

/// <summary>
/// Handles the LearnerNotInCar domain event.
///
/// Resolves any open gap for this learner — the driver has made an
/// accountable statement, so the gap closes as *explained*.
/// </summary>
public sealed class LearnerNotInCarHandler
    : INotificationHandler<DomainEventNotification<LearnerNotInCar>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public LearnerNotInCarHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(
        DomainEventNotification<LearnerNotInCar> notification,
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
using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Events;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;
using SafePick.Domain.Custody.Events;

namespace SafePick.Application.Custody.EventHandlers;

/// <summary>
/// Handles the SchoolMarkedAbsent domain event.
///
/// Resolves any open gap for this learner — the absence makes the gap
/// unnecessary, and the gap closes with a reference to the absence event.
/// </summary>
public sealed class SchoolMarkedAbsentHandler
    : INotificationHandler<DomainEventNotification<SchoolMarkedAbsent>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public SchoolMarkedAbsentHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(
        DomainEventNotification<SchoolMarkedAbsent> notification,
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
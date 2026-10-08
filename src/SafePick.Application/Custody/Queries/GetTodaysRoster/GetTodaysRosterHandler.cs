using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Custody.Queries.GetTodaysRoster;

public sealed class GetTodaysRosterHandler
    : IRequestHandler<GetTodaysRosterQuery, IReadOnlyList<RosterItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTodaysRosterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RosterItemDto>> Handle(
        GetTodaysRosterQuery request,
        CancellationToken cancellationToken)
    {
        var dayStart = request.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        // Active assignments for this driver.
        var assignments = await _context.Assignments
            .Where(a => a.DriverId == request.DriverId && a.ValidTo == null)
            .ToListAsync(cancellationToken);

        var learnerIds = assignments.Select(a => a.LearnerId).ToList();

        var learners = await _context.Learners
            .Where(l => learnerIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var vehicleIds = assignments.Select(a => a.VehicleId).Distinct().ToList();

        var vehicles = await _context.Drivers
            .Where(d => d.Id == request.DriverId)
            .Select(d => new { d.Vehicle.Id, d.Vehicle.RegistrationNumber })
            .ToDictionaryAsync(v => v.Id, v => v.RegistrationNumber, cancellationToken);

        // Custody events recorded today for these learners by this driver.
        //
        // The comparison wraps the boundaries in ServerTimestamp value
        // objects so EF Core's value converter translates the comparison
        // into SQL against the underlying server_timestamp column.
        var dayStartTs = new ServerTimestamp(dayStart);
        var dayEndTs = new ServerTimestamp(dayEnd);

        var events = await _context.CustodyEvents
            .Where(e =>
                e.DriverId == request.DriverId &&
                learnerIds.Contains(e.LearnerId) &&
                e.ServerTimestamp >= dayStartTs &&
                e.ServerTimestamp < dayEndTs)
            .ToListAsync(cancellationToken);

        var results = new List<RosterItemDto>(assignments.Count);

        foreach (var a in assignments)
        {
            learners.TryGetValue(a.LearnerId, out var learner);

            var latestEvent = events
                .Where(e => e.LearnerId == a.LearnerId)
                .OrderByDescending(e => e.ServerTimestamp.Value)
                .FirstOrDefault();

            results.Add(new RosterItemDto(
                LearnerId: a.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "(unknown)",
                AssignmentId: a.Id,
                SchoolId: a.SchoolId,
                VehicleId: a.VehicleId,
                VehicleRegistration: vehicles.TryGetValue(a.VehicleId, out var reg)
                    ? reg : "(unknown)",
                Status: MapStatus(latestEvent),
                CustodyEventId: latestEvent?.Id,
                ResolvedAt: latestEvent?.ServerTimestamp.Value,
                Note: latestEvent?.Reason));
        }

        return results
            .OrderBy(r => r.ClassGroup)
            .ThenBy(r => r.LearnerFullName)
            .ToList();
    }

    private static CustodyStatusDto MapStatus(CustodyEvent? evt)
    {
        if (evt is null) return CustodyStatusDto.Pending;

        return evt.Type switch
        {
            CustodyEventType.PickedUp => CustodyStatusDto.PickedUp,
            CustodyEventType.NotInCar => CustodyStatusDto.NotInCar,
            CustodyEventType.SchoolMarkedAbsent => CustodyStatusDto.SchoolMarkedAbsent,
            CustodyEventType.Correction => CustodyStatusDto.Corrected,
            _ => CustodyStatusDto.Pending
        };
    }
}
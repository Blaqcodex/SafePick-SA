using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Custody.Queries.GetExpectedPickupsForWindow;

public sealed class GetExpectedPickupsForWindowHandler
    : IRequestHandler<GetExpectedPickupsForWindowQuery, IReadOnlyList<ExpectedPickupDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpectedPickupsForWindowHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExpectedPickupDto>> Handle(
        GetExpectedPickupsForWindowQuery request,
        CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == request.SchoolId, cancellationToken)
            ?? throw new NotFoundException(nameof(School), request.SchoolId);

        var window = school.TransportWindows.FirstOrDefault()
            ?? throw new DomainException(
                "School has no transport window configured; cannot derive expectations.");

        var dayStart = request.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        // Active assignments for this school.
        var assignments = await _context.Assignments
            .Where(a => a.SchoolId == request.SchoolId && a.ValidTo == null)
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
            return Array.Empty<ExpectedPickupDto>();

        var learnerIds = assignments.Select(a => a.LearnerId).Distinct().ToList();
        var driverIds = assignments.Select(a => a.DriverId).Distinct().ToList();

        var learners = await _context.Learners
            .Where(l => learnerIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .Select(d => new
            {
                d.Id,
                d.FullName,
                VehicleId = d.Vehicle.Id,
                Registration = d.Vehicle.RegistrationNumber
            })
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        // Custody events for the day that could resolve an expectation.
        //
        // The comparison wraps the boundaries in ServerTimestamp value
        // objects so EF Core's value converter translates the comparison
        // into SQL against the underlying server_timestamp column.
        var dayStartTs = new ServerTimestamp(dayStart);
        var dayEndTs = new ServerTimestamp(dayEnd);

        var events = await _context.CustodyEvents
            .Where(e =>
                e.SchoolId == request.SchoolId &&
                learnerIds.Contains(e.LearnerId) &&
                e.ServerTimestamp >= dayStartTs &&
                e.ServerTimestamp < dayEndTs)
            .ToListAsync(cancellationToken);

        var results = new List<ExpectedPickupDto>();

        foreach (var a in assignments)
        {
            // Derive the expectation via the domain.
            var expectation = CustodyExpectation.Derive(a, window, request.Date);

            // Find a resolving event. This runs in memory after materialization.
            var resolving = events
                .Where(e => e.LearnerId == a.LearnerId)
                .Where(e => expectation.IsResolvedBy(e.ServerTimestamp.Value))
                .OrderByDescending(e => e.ServerTimestamp.Value)
                .FirstOrDefault();

            learners.TryGetValue(a.LearnerId, out var learner);
            drivers.TryGetValue(a.DriverId, out var driver);

            results.Add(new ExpectedPickupDto(
                SchoolId: a.SchoolId,
                LearnerId: a.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "(unknown)",
                AssignmentId: a.Id,
                DriverId: a.DriverId,
                DriverName: driver?.FullName ?? "(unknown)",
                VehicleId: a.VehicleId,
                VehicleRegistration: driver?.Registration ?? "(unknown)",
                WindowStart: expectation.WindowStart,
                WindowEnd: expectation.WindowEnd,
                ReconciliationAt: expectation.ReconciliationAt,
                IsResolved: resolving is not null,
                ResolvedByEventId: resolving?.Id,
                ResolutionType: resolving?.Type.ToString()));
        }

        return results
            .OrderBy(r => r.ClassGroup)
            .ThenBy(r => r.LearnerFullName)
            .ToList();
    }
}
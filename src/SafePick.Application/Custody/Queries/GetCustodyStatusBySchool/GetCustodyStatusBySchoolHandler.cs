using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Custody.Queries.GetCustodyStatusBySchool;

public sealed class GetCustodyStatusBySchoolHandler
    : IRequestHandler<GetCustodyStatusBySchoolQuery, SchoolCustodyBoardDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustodyStatusBySchoolHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolCustodyBoardDto> Handle(
        GetCustodyStatusBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var dayStart = request.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        // Active assignments for the school today.
        var assignments = await _context.Assignments
            .Where(a => a.SchoolId == request.SchoolId && a.ValidTo == null)
            .ToListAsync(cancellationToken);

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

        // Custody events for the school today.
        //
        // The comparison wraps the boundaries in ServerTimestamp value
        // objects so EF Core's value converter translates the comparison
        // into SQL against the underlying server_timestamp column.
        var dayStartTs = new ServerTimestamp(dayStart);
        var dayEndTs = new ServerTimestamp(dayEnd);

        var events = await _context.CustodyEvents
            .Where(e =>
                e.SchoolId == request.SchoolId &&
                e.ServerTimestamp >= dayStartTs &&
                e.ServerTimestamp < dayEndTs)
            .ToListAsync(cancellationToken);

        var openGaps = await _context.CustodyGaps
            .CountAsync(g =>
                g.SchoolId == request.SchoolId &&
                g.State != GapState.Closed &&
                g.State != GapState.Resolved,
                cancellationToken);

        var rows = new List<CustodyBoardRow>();
        int pickedUp = 0, notInCar = 0, absent = 0, pending = 0;

        foreach (var a in assignments)
        {
            learners.TryGetValue(a.LearnerId, out var learner);
            drivers.TryGetValue(a.DriverId, out var driver);

            var latestEvent = events
                .Where(e => e.LearnerId == a.LearnerId)
                .OrderByDescending(e => e.ServerTimestamp.Value)
                .FirstOrDefault();

            var status = MapStatus(latestEvent);

            switch (status)
            {
                case "PickedUp": pickedUp++; break;
                case "NotInCar": notInCar++; break;
                case "SchoolMarkedAbsent": absent++; break;
                default: pending++; break;
            }

            rows.Add(new CustodyBoardRow(
                LearnerId: a.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "(unknown)",
                DriverId: a.DriverId,
                DriverName: driver?.FullName,
                VehicleId: a.VehicleId,
                VehicleRegistration: driver?.Registration,
                Status: status,
                CustodyEventId: latestEvent?.Id,
                ResolvedAt: latestEvent?.ServerTimestamp.Value));
        }

        return new SchoolCustodyBoardDto(
            SchoolId: request.SchoolId,
            Date: request.Date,
            TotalExpected: assignments.Count,
            PickedUp: pickedUp,
            NotInCar: notInCar,
            MarkedAbsent: absent,
            Pending: pending,
            OpenGaps: openGaps,
            Rows: rows.OrderBy(r => r.ClassGroup).ThenBy(r => r.LearnerFullName).ToList());
    }

    private static string MapStatus(CustodyEvent? evt)
    {
        if (evt is null) return "Pending";

        return evt.Type switch
        {
            CustodyEventType.PickedUp => "PickedUp",
            CustodyEventType.NotInCar => "NotInCar",
            CustodyEventType.SchoolMarkedAbsent => "SchoolMarkedAbsent",
            CustodyEventType.Correction => "Corrected",
            _ => "Pending"
        };
    }
}
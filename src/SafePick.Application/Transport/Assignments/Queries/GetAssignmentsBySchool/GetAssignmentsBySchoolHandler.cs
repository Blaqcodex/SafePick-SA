using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Assignments.Queries.GetAssignmentsBySchool;

public sealed class GetAssignmentsBySchoolHandler
    : IRequestHandler<GetAssignmentsBySchoolQuery, IReadOnlyList<AssignmentItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignmentsBySchoolHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AssignmentItemDto>> Handle(
        GetAssignmentsBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Assignments
            .Where(a => a.SchoolId == request.SchoolId);

        if (!request.IncludeTerminated)
        {
            query = query.Where(a => a.ValidTo == null);
        }

        var assignments = await query
            .OrderByDescending(a => a.ValidFrom)
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return Array.Empty<AssignmentItemDto>();
        }

        var learnerIds = assignments.Select(a => a.LearnerId).Distinct().ToList();
        var driverIds  = assignments.Select(a => a.DriverId).Distinct().ToList();

        var learners = await _context.Learners
            .Where(l => learnerIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .Include(d => d.Vehicle)
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var results = new List<AssignmentItemDto>(assignments.Count);

        foreach (var a in assignments)
        {
            learners.TryGetValue(a.LearnerId, out var learner);
            drivers.TryGetValue(a.DriverId, out var driver);

            results.Add(new AssignmentItemDto(
                AssignmentId: a.Id,
                SchoolId: a.SchoolId,
                LearnerId: a.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "—",
                DriverId: a.DriverId,
                DriverFullName: driver?.FullName ?? "(unknown)",
                VehicleId: a.VehicleId,
                VehicleRegistration: driver?.Vehicle.RegistrationNumber ?? "—",
                ValidFrom: a.ValidFrom,
                ValidTo: a.ValidTo));
        }

        return results;
    }
}
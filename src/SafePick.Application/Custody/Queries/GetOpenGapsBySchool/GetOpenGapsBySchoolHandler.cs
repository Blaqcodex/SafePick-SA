using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Custody;

namespace SafePick.Application.Custody.Queries.GetOpenGapsBySchool;

public sealed class GetOpenGapsBySchoolHandler
    : IRequestHandler<GetOpenGapsBySchoolQuery, IReadOnlyList<CustodyGapDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOpenGapsBySchoolHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustodyGapDto>> Handle(
        GetOpenGapsBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.CustodyGaps
            .Where(g => g.SchoolId == request.SchoolId);

        if (!request.IncludeResolved)
        {
            query = query.Where(g =>
                g.State != GapState.Resolved &&
                g.State != GapState.Closed);
        }

        var gaps = await query
            .OrderBy(g => g.OpenedAt)
            .ToListAsync(cancellationToken);

        var learnerIds = gaps.Select(g => g.LearnerId).Distinct().ToList();
        var assignmentIds = gaps.Select(g => g.AssignmentId).Distinct().ToList();

        var learners = await _context.Learners
            .Where(l => learnerIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var assignments = await _context.Assignments
            .Where(a => assignmentIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var driverIds = assignments.Values.Select(a => a.DriverId).Distinct().ToList();

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.FullName, cancellationToken);

        return gaps.Select(g =>
        {
            learners.TryGetValue(g.LearnerId, out var learner);
            assignments.TryGetValue(g.AssignmentId, out var assignment);

            Guid? driverId = assignment?.DriverId;
            string? driverName = driverId.HasValue &&
                                 drivers.TryGetValue(driverId.Value, out var name)
                ? name : null;

            return new CustodyGapDto(
                GapId: g.Id,
                LearnerId: g.LearnerId,
                LearnerFullName: learner?.FullName ?? "(unknown)",
                ClassGroup: learner?.ClassGroup.ToString() ?? "(unknown)",
                AssignmentId: g.AssignmentId,
                DriverId: driverId,
                DriverName: driverName,
                State: g.State.ToString(),
                ExpectedWindowStart: g.ExpectedWindowStart,
                ExpectedWindowEnd: g.ExpectedWindowEnd,
                OpenedAt: g.OpenedAt,
                AcknowledgedByUserId: g.AcknowledgedByUserId,
                AcknowledgedAt: g.AcknowledgedAt,
                ResolvedAt: g.ResolvedAt,
                Resolution: g.Resolution);
        }).ToList();
    }
}
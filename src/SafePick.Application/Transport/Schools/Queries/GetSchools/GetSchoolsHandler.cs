using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Schools.Queries.GetSchools;

public sealed class GetSchoolsHandler
    : IRequestHandler<GetSchoolsQuery, IReadOnlyList<SchoolItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolItemDto>> Handle(
        GetSchoolsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Schools.AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var schools = await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        if (schools.Count == 0)
        {
            return Array.Empty<SchoolItemDto>();
        }

        // Per-school counts. Learners and drivers are separate aggregates.
        var schoolIds = schools.Select(s => s.Id).ToList();

        var learnerCounts = await _context.Learners
            .Where(l => schoolIds.Contains(l.SchoolId) && l.IsActive)
            .GroupBy(l => l.SchoolId)
            .Select(g => new { SchoolId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SchoolId, x => x.Count, cancellationToken);

        // Driver counts: count of Approved DriverApprovals per school.
        var driverCounts = await _context.DriverApprovals
            .Where(a => schoolIds.Contains(a.SchoolId) &&
                        a.Status == Domain.Transport.DriverApproval.ApprovalStatus.Approved)
            .GroupBy(a => a.SchoolId)
            .Select(g => new { SchoolId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SchoolId, x => x.Count, cancellationToken);

        return schools.Select(s => new SchoolItemDto(
            SchoolId: s.Id,
            Name: s.Name,
            ContactEmail: s.ContactEmail,
            IsActive: s.IsActive,
            LearnerCount: learnerCounts.TryGetValue(s.Id, out var lc) ? lc : 0,
            DriverCount: driverCounts.TryGetValue(s.Id, out var dc) ? dc : 0
        )).ToList();
    }
}
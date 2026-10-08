using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Learners.Queries.GetLearnersBySchool;

public sealed class GetLearnersBySchoolHandler
    : IRequestHandler<GetLearnersBySchoolQuery, IReadOnlyList<LearnerItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLearnersBySchoolHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LearnerItemDto>> Handle(
        GetLearnersBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Learners
            .Where(l => l.SchoolId == request.SchoolId);

        if (!request.IncludeInactive)
        {
            query = query.Where(l => l.IsActive);
        }

        var learners = await query
            .OrderBy(l => l.FullName)
            .ToListAsync(cancellationToken);

        if (learners.Count == 0)
        {
            return Array.Empty<LearnerItemDto>();
        }

        var learnerIds = learners.Select(l => l.Id).ToList();

        // Fetch current assignments for these learners (validTo IS NULL).
        var assignments = await _context.Assignments
            .Where(a => learnerIds.Contains(a.LearnerId) && a.ValidTo == null)
            .ToListAsync(cancellationToken);

        // Fetch the drivers and vehicles referenced by these assignments.
        var driverIds = assignments.Select(a => a.DriverId).Distinct().ToList();

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .Include(d => d.Vehicle)
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var assignmentsByLearner = assignments
            .GroupBy(a => a.LearnerId)
            .ToDictionary(g => g.Key, g => g.First());

        var results = new List<LearnerItemDto>(learners.Count);

        foreach (var learner in learners)
        {
            assignmentsByLearner.TryGetValue(learner.Id, out var assignment);

            string? driverName = null;
            string? vehicleReg = null;

            if (assignment is not null && drivers.TryGetValue(assignment.DriverId, out var driver))
            {
                driverName = driver.FullName;
                vehicleReg = driver.Vehicle.RegistrationNumber;
            }

            results.Add(new LearnerItemDto(
                LearnerId: learner.Id,
                SchoolId: learner.SchoolId,
                FullName: learner.FullName,
                GradeClass: new GradeClassDto(
                    Grade: learner.ClassGroup.Grade,
                    Letter: learner.ClassGroup.Letter),
                ParentContact: learner.ParentContact is null
                    ? null
                    : new ParentContactDto(
                        Email: learner.ParentContact.Email,
                        Phone: learner.ParentContact.Phone,
                        Name: learner.ParentContact.Name,
                        ConsentRecorded: learner.ParentContact.ConsentRecorded),
                IsActive: learner.IsActive,
                CurrentAssignmentId: assignment?.Id,
                CurrentDriverId: assignment?.DriverId,
                CurrentDriverName: driverName,
                CurrentVehicleRegistration: vehicleReg));
        }

        return results;
    }
}
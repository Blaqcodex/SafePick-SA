using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;
using SafePick.Domain.Common;

namespace SafePick.Application.Transport.Assignments.Commands.AssignLearnerToVehicle;

/// <summary>
/// Assigns a learner to a driver's vehicle. Enforces all cross-aggregate
/// invariants:
///   1. The learner exists and can be assigned (active + has a parent contact).
///   2. The driver exists and is approved by the learner's school.
///   3. The learner has no other *open* assignment.
///   4. The vehicle has capacity available.
/// </summary>
public sealed class AssignLearnerToVehicleHandler
    : IRequestHandler<AssignLearnerToVehicleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public AssignLearnerToVehicleHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        AssignLearnerToVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var validFrom = request.ValidFrom ?? now;

        // 1. Learner
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == request.LearnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Learner), request.LearnerId);

        if (!learner.CanBeAssigned())
            throw new DomainException(
                "Learner must be active and have a parent contact before assignment.");

        // 2. Driver approved by the learner's school
        var approval = await _context.DriverApprovals
            .FirstOrDefaultAsync(a =>
                a.DriverId == request.DriverId &&
                a.SchoolId == learner.SchoolId &&
                a.Status == DriverApproval.ApprovalStatus.Approved,
                cancellationToken)
            ?? throw new DomainException(
                "Driver is not approved for this learner's school.");

        var driver = await _context.Drivers
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken)
            ?? throw new NotFoundException(nameof(Driver), request.DriverId);

        // 3. No open assignment already
        var hasOpenAssignment = await _context.Assignments
            .AnyAsync(a =>
                a.LearnerId == learner.Id &&
                a.ValidTo == null,
                cancellationToken);

        if (hasOpenAssignment)
            throw new DomainException(
                "Learner already has an open assignment. Reassign instead.");

        // 4. Vehicle capacity
        var activeCount = await _context.Assignments
            .CountAsync(a =>
                a.VehicleId == driver.Vehicle.Id &&
                a.ValidTo == null,
                cancellationToken);

        if (activeCount >= driver.Vehicle.Capacity)
            throw new DomainException(
                $"Vehicle capacity ({driver.Vehicle.Capacity}) has been reached.");

        var assignedByUserId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        var assignment = Assignment.Create(
            learner.SchoolId,
            learner.Id,
            driver.Vehicle.Id,
            driver.Id,
            validFrom,
            assignedByUserId,
            now);

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
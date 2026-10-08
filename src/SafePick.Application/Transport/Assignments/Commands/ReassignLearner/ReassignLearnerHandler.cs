using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;
using SafePick.Domain.Common;

namespace SafePick.Application.Transport.Assignments.Commands.ReassignLearner;

public sealed class ReassignLearnerHandler
    : IRequestHandler<ReassignLearnerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public ReassignLearnerHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        ReassignLearnerCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var validFrom = request.ValidFrom ?? now;

        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == request.LearnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Learner), request.LearnerId);

        if (!learner.CanBeAssigned())
            throw new DomainException(
                "Learner must be active and have a parent contact before reassignment.");

        var approval = await _context.DriverApprovals
            .FirstOrDefaultAsync(a =>
                a.DriverId == request.NewDriverId &&
                a.SchoolId == learner.SchoolId &&
                a.Status == DriverApproval.ApprovalStatus.Approved,
                cancellationToken)
            ?? throw new DomainException(
                "New driver is not approved for this learner's school.");

        var driver = await _context.Drivers
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.Id == request.NewDriverId, cancellationToken)
            ?? throw new NotFoundException(nameof(Driver), request.NewDriverId);

        var activeCount = await _context.Assignments
            .CountAsync(a => a.VehicleId == driver.Vehicle.Id && a.ValidTo == null,
                cancellationToken);

        if (activeCount >= driver.Vehicle.Capacity)
            throw new DomainException(
                $"Vehicle capacity ({driver.Vehicle.Capacity}) has been reached.");

        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        // Terminate the current open assignment (if any).
        var current = await _context.Assignments
            .FirstOrDefaultAsync(a => a.LearnerId == learner.Id && a.ValidTo == null,
                cancellationToken);

        if (current is not null)
            current.Terminate(validFrom, userId, now);

        // Create the new one.
        var assignment = Assignment.Create(
            learner.SchoolId,
            learner.Id,
            driver.Vehicle.Id,
            driver.Id,
            validFrom,
            userId,
            now);

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
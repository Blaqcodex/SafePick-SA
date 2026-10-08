using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Custody.Commands.ConfirmPickup;

public sealed class ConfirmPickupHandler : IRequestHandler<ConfirmPickupCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;

    public ConfirmPickupHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        ConfirmPickupCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Idempotency: if an event with this ClientGeneratedId already
        //    exists, return its id. This makes the endpoint safe to retry.
        var existing = await _context.CustodyEvents
            .FirstOrDefaultAsync(e =>
                e.ClientGeneratedId == request.ClientGeneratedId,
                cancellationToken);

        if (existing is not null)
            return existing.Id;

        // 2. Resolve the active assignment for the learner.
        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a =>
                a.LearnerId == request.LearnerId &&
                a.DriverId == request.DriverId &&
                a.ValidTo == null,
                cancellationToken)
            ?? throw new DomainException(
                "No active assignment found for this learner and driver.");

        // 3. Confirm the driver is approved by the learner's school.
        var approved = await _context.DriverApprovals
            .AnyAsync(a =>
                a.DriverId == request.DriverId &&
                a.SchoolId == assignment.SchoolId &&
                a.Status == DriverApproval.ApprovalStatus.Approved,
                cancellationToken);

        if (!approved)
            throw new DomainException(
                "Driver is not approved for this school.");

        // 4. Record the pickup.
        var now = _clock.UtcNow;
        var serverTimestamp = new ServerTimestamp(now);

        var evt = CustodyEvent.RecordPickup(
            schoolId: assignment.SchoolId,
            learnerId: request.LearnerId,
            driverId: request.DriverId,
            vehicleId: assignment.VehicleId,
            assignmentId: assignment.Id,
            clientGeneratedId: request.ClientGeneratedId,
            serverTimestamp: serverTimestamp,
            deviceTimestamp: request.DeviceTimestamp,
            now: now);

        _context.CustodyEvents.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        return evt.Id;
    }
}
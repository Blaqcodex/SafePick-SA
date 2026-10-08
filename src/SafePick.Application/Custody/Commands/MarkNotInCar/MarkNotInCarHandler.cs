using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Custody.Commands.MarkNotInCar;

public sealed class MarkNotInCarHandler : IRequestHandler<MarkNotInCarCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public MarkNotInCarHandler(IApplicationDbContext context, IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        MarkNotInCarCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _context.CustodyEvents
            .FirstOrDefaultAsync(e =>
                e.ClientGeneratedId == request.ClientGeneratedId,
                cancellationToken);

        if (existing is not null)
            return existing.Id;

        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a =>
                a.LearnerId == request.LearnerId &&
                a.DriverId == request.DriverId &&
                a.ValidTo == null,
                cancellationToken)
            ?? throw new DomainException(
                "No active assignment found for this learner and driver.");

        var approved = await _context.DriverApprovals
            .AnyAsync(a =>
                a.DriverId == request.DriverId &&
                a.SchoolId == assignment.SchoolId &&
                a.Status == DriverApproval.ApprovalStatus.Approved,
                cancellationToken);

        if (!approved)
            throw new DomainException("Driver is not approved for this school.");

        var now = _clock.UtcNow;
        var serverTimestamp = new ServerTimestamp(now);

        var evt = CustodyEvent.RecordNotInCar(
            schoolId: assignment.SchoolId,
            learnerId: request.LearnerId,
            driverId: request.DriverId,
            vehicleId: assignment.VehicleId,
            assignmentId: assignment.Id,
            clientGeneratedId: request.ClientGeneratedId,
            reason: request.Reason,
            serverTimestamp: serverTimestamp,
            deviceTimestamp: request.DeviceTimestamp,
            now: now);

        _context.CustodyEvents.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        return evt.Id;
    }
}
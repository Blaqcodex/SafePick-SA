using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;

namespace SafePick.Application.Custody.Commands.BatchConfirmPickups;

public sealed class BatchConfirmPickupsHandler
    : IRequestHandler<BatchConfirmPickupsCommand, BatchConfirmPickupsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public BatchConfirmPickupsHandler(
        IApplicationDbContext context, IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<BatchConfirmPickupsResult> Handle(
        BatchConfirmPickupsCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var outcomes = new List<BatchPickupOutcome>(request.Items.Count);

        foreach (var item in request.Items)
        {
            try
            {
                var existing = await _context.CustodyEvents
                    .FirstOrDefaultAsync(e =>
                        e.ClientGeneratedId == item.ClientGeneratedId,
                        cancellationToken);

                if (existing is not null)
                {
                    outcomes.Add(new BatchPickupOutcome(
                        item.LearnerId, true, existing.Id, null));
                    continue;
                }

                var assignment = await _context.Assignments
                    .FirstOrDefaultAsync(a =>
                        a.LearnerId == item.LearnerId &&
                        a.DriverId == request.DriverId &&
                        a.ValidTo == null,
                        cancellationToken);

                if (assignment is null)
                {
                    outcomes.Add(new BatchPickupOutcome(
                        item.LearnerId, false, null,
                        "No active assignment for this learner and driver."));
                    continue;
                }

                var approved = await _context.DriverApprovals
                    .AnyAsync(a =>
                        a.DriverId == request.DriverId &&
                        a.SchoolId == assignment.SchoolId &&
                        a.Status == DriverApproval.ApprovalStatus.Approved,
                        cancellationToken);

                if (!approved)
                {
                    outcomes.Add(new BatchPickupOutcome(
                        item.LearnerId, false, null,
                        "Driver is not approved for this school."));
                    continue;
                }

                var evt = CustodyEvent.RecordPickup(
                    schoolId: assignment.SchoolId,
                    learnerId: item.LearnerId,
                    driverId: request.DriverId,
                    vehicleId: assignment.VehicleId,
                    assignmentId: assignment.Id,
                    clientGeneratedId: item.ClientGeneratedId,
                    serverTimestamp: new ServerTimestamp(now),
                    deviceTimestamp: item.DeviceTimestamp,
                    now: now);

                _context.CustodyEvents.Add(evt);
                await _context.SaveChangesAsync(cancellationToken);

                outcomes.Add(new BatchPickupOutcome(
                    item.LearnerId, true, evt.Id, null));
            }
            catch (Exception ex)
            {
                outcomes.Add(new BatchPickupOutcome(
                    item.LearnerId, false, null, ex.Message));
            }
        }

        return new BatchConfirmPickupsResult(outcomes);
    }
}
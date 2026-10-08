using MediatR;

namespace SafePick.Application.Custody.Commands.BatchConfirmPickups;

/// <summary>
/// Confirms multiple learners in a single call — the driver's "confirm all"
/// action at the end of the pickup window.
///
/// Partial success is reported per learner. A failure on one learner
/// (e.g., not assigned to this driver) does not roll back the others —
/// each pickup is its own atomic operation, and each result is returned.
/// </summary>
public sealed record BatchConfirmPickupsCommand(
    Guid SchoolId,
    Guid DriverId,
    IReadOnlyList<BatchPickupItem> Items) : IRequest<BatchConfirmPickupsResult>;

public sealed record BatchPickupItem(
    Guid LearnerId,
    Guid ClientGeneratedId,
    DateTime? DeviceTimestamp);

public sealed record BatchConfirmPickupsResult(
    IReadOnlyList<BatchPickupOutcome> Outcomes);

public sealed record BatchPickupOutcome(
    Guid LearnerId,
    bool Succeeded,
    Guid? CustodyEventId,
    string? Error);
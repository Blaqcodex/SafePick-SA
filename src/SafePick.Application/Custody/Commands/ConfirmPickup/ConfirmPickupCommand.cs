using MediatR;

namespace SafePick.Application.Custody.Commands.ConfirmPickup;

/// <summary>
/// Confirms that a learner was picked up.
///
/// The ClientGeneratedId is provided by the caller (the driver's device)
/// and is used to deduplicate retries. Calling this command twice with
/// the same ClientGeneratedId returns the original event id rather than
/// creating a duplicate.
/// </summary>
public sealed record ConfirmPickupCommand(
    Guid SchoolId,
    Guid LearnerId,
    Guid DriverId,
    Guid ClientGeneratedId,
    DateTime? DeviceTimestamp) : IRequest<Guid>;
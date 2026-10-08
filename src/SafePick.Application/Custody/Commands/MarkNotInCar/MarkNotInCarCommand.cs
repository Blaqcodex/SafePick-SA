using MediatR;

namespace SafePick.Application.Custody.Commands.MarkNotInCar;

/// <summary>
/// Marks a learner as "not in the car" — the learner was expected but
/// was not collected. A reason is mandatory; the driver must provide
/// an accountable statement.
/// </summary>
public sealed record MarkNotInCarCommand(
    Guid SchoolId,
    Guid LearnerId,
    Guid DriverId,
    Guid ClientGeneratedId,
    string Reason,
    DateTime? DeviceTimestamp) : IRequest<Guid>;
using MediatR;

namespace SafePick.Application.Transport.Assignments.Commands.AssignLearnerToVehicle;

public sealed record AssignLearnerToVehicleCommand(
    Guid LearnerId,
    Guid DriverId,
    DateTime? ValidFrom) : IRequest<Guid>;
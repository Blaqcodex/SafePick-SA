using MediatR;

namespace SafePick.Application.Transport.Assignments.Commands.ReassignLearner;

/// <summary>
/// Reassigns a learner from their current driver to a new driver.
/// Performed atomically: the current assignment is terminated and a new
/// one is created in a single transaction.
/// </summary>
public sealed record ReassignLearnerCommand(
    Guid LearnerId,
    Guid NewDriverId,
    DateTime? ValidFrom) : IRequest<Guid>;
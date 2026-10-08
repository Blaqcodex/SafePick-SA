using MediatR;

namespace SafePick.Application.Transport.Assignments.Commands.UnassignLearner;

public sealed record UnassignLearnerCommand(Guid AssignmentId, DateTime? TerminatedAt) : IRequest;
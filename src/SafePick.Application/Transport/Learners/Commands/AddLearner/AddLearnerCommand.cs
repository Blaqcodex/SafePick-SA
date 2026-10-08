using MediatR;

namespace SafePick.Application.Transport.Learners.Commands.AddLearner;

public sealed record AddLearnerCommand(
    Guid SchoolId,
    string FullName,
    int Grade,
    char ClassLetter) : IRequest<Guid>;
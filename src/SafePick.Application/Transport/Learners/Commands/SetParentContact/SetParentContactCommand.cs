using MediatR;

namespace SafePick.Application.Transport.Learners.Commands.SetParentContact;

public sealed record SetParentContactCommand(
    Guid LearnerId,
    string Email,
    string? Phone,
    string? Name,
    bool ConsentRecorded) : IRequest;
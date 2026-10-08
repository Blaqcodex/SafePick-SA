using MediatR;

namespace SafePick.Application.Transport.Drivers.Commands.RejectDriver;

public sealed record RejectDriverCommand(Guid ApprovalId, string Reason) : IRequest;
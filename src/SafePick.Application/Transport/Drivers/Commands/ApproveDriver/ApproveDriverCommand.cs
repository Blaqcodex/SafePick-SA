using MediatR;

namespace SafePick.Application.Transport.Drivers.Commands.ApproveDriver;

public sealed record ApproveDriverCommand(Guid ApprovalId) : IRequest;
using MediatR;

namespace SafePick.Application.Transport.Drivers.Commands.RevokeDriverApproval;

public sealed record RevokeDriverApprovalCommand(Guid ApprovalId, string Reason) : IRequest;
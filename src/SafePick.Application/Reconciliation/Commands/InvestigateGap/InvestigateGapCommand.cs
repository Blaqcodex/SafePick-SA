using MediatR;

namespace SafePick.Application.Reconciliation.Commands.InvestigateGap;

/// <summary>
/// Moves a gap into the Investigating state. Only valid from
/// Acknowledged. Signals that the school is actively determining
/// what happened.
/// </summary>
public sealed record InvestigateGapCommand(Guid GapId) : IRequest;
using MediatR;

namespace SafePick.Application.Reconciliation.Commands.AcknowledgeGap;

/// <summary>
/// Acknowledges a custody gap. Only valid from the Open state.
///
/// Acknowledgement means a school admin has seen the gap and taken
/// responsibility for driving it to resolution. It does not resolve
/// the gap — it starts the process.
/// </summary>
public sealed record AcknowledgeGapCommand(Guid GapId) : IRequest;
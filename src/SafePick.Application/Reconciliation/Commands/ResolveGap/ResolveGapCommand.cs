using MediatR;

namespace SafePick.Application.Reconciliation.Commands.ResolveGap;

/// <summary>
/// Resolves a custody gap with a recorded explanation. Valid from
/// Acknowledged or Investigating.
///
/// A resolution reason is mandatory — the record must explain what
/// happened. This is what closes the loop of accountability.
/// </summary>
public sealed record ResolveGapCommand(
    Guid GapId,
    string Resolution,
    Guid? ResolvedByCustodyEventId = null) : IRequest;
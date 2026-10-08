using MediatR;

namespace SafePick.Application.Reconciliation.Commands.RunReconciliation;

/// <summary>
/// Runs reconciliation for a school on a given school day.
///
/// Reconciliation derives every expectation, evaluates each against the
/// custody events recorded for that day, and opens a gap for every
/// expectation that was not resolved. Every outcome is recorded.
///
/// This is the mechanism by which the system guarantees that no
/// expected pickup is ever silently unaccounted for.
///
/// When DryRun is true, the handler evaluates and returns what it would
/// do, but opens no gaps and persists no run. Useful for demonstration
/// and for safely inspecting a live system.
/// </summary>
public sealed record RunReconciliationCommand(
    Guid SchoolId,
    DateOnly SchoolDay,
    bool DryRun = false) : IRequest<RunReconciliationResult>;
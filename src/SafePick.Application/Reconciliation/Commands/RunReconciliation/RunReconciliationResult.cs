namespace SafePick.Application.Reconciliation.Commands.RunReconciliation;

/// <summary>
/// The result of a reconciliation run: aggregate counts and the
/// per-learner outcomes, so a caller can see exactly what happened.
/// </summary>
public sealed record RunReconciliationResult(
    Guid? ReconciliationRunId,
    Guid SchoolId,
    DateOnly SchoolDay,
    bool DryRun,
    int ExpectationsEvaluated,
    int ResolvedCount,
    int SuppressedCount,
    int GappedCount,
    IReadOnlyList<ReconciliationItemResult> Items);

public sealed record ReconciliationItemResult(
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid AssignmentId,
    string Outcome,
    Guid? CustodyEventId,
    Guid? GapId);
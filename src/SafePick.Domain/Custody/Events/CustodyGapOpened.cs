using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when the reconciliation engine determines that an expectation
/// was not resolved by window close (plus grace period), and no custody
/// event explains the absence.
///
/// A gap is a first-class, owned, auditable representation of
/// "we expected a handover and cannot confirm it." It never silently
/// disappears — it must be acknowledged, investigated, and resolved.
/// </summary>
public sealed record CustodyGapOpened(
    Guid GapId,
    Guid SchoolId,
    Guid LearnerId,
    Guid AssignmentId,
    Guid ReconciliationRunId,
    DateTime ExpectedWindowStart,
    DateTime ExpectedWindowEnd,
    DateTime OccurredOn) : IDomainEvent;
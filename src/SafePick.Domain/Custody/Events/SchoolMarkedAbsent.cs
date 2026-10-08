using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a school marks a learner as absent before the pickup
/// window opens. This suppresses the expectation for that day — the
/// learner was not expected to be collected.
///
/// The suppression is explicit and auditable. A learner who is later
/// picked up anyway (because the school's absence was mistaken) will
/// have both events, and reconciliation will reconcile them.
/// </summary>
public sealed record SchoolMarkedAbsent(
    Guid CustodyEventId,
    Guid SchoolId,
    Guid LearnerId,
    Guid MarkedByUserId,
    string? Note,
    DateTime ServerTimestamp,
    DateTime OccurredOn) : IDomainEvent;
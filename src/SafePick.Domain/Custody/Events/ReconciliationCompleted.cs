using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a reconciliation run completes.
///
/// This event is the system's record that it evaluated every expectation
/// for a school day. It carries the counts, so downstream consumers
/// (notification, monitoring) can react without re-querying the run.
/// </summary>
public sealed record ReconciliationCompleted(
    Guid ReconciliationRunId,
    Guid SchoolId,
    DateOnly SchoolDay,
    int ExpectationsEvaluated,
    int ResolvedCount,
    int SuppressedCount,
    int GappedCount,
    DateTime OccurredOn) : IDomainEvent;
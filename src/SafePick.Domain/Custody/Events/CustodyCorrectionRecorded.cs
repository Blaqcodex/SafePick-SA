using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a correction is recorded against a prior custody event.
///
/// The original event is never modified. The correction is a new event
/// that references the original by id, states what was wrong, and states
/// what the corrected state should be. Reconciliation and reporting
/// consider the correction when evaluating custody status.
///
/// This is ADR-0011 in code: corrections are compensating events, not
/// mutations.
/// </summary>
public sealed record CustodyCorrectionRecorded(
    Guid CorrectionEventId,
    Guid OriginalEventId,
    Guid SchoolId,
    Guid LearnerId,
    Guid CorrectedByUserId,
    string Reason,
    DateTime ServerTimestamp,
    DateTime OccurredOn) : IDomainEvent;
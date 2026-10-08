using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a custody gap is resolved — either because a late custody
/// event arrived (e.g., an offline driver synced), or because a human
/// determined the correct outcome.
///
/// The gap is not deleted. It is marked Resolved, with the resolving
/// event and the resolver recorded for audit.
/// </summary>
public sealed record CustodyGapResolved(
    Guid GapId,
    Guid SchoolId,
    Guid LearnerId,
    Guid? ResolvedByCustodyEventId,
    Guid? ResolvedByUserId,
    string Resolution,
    DateTime OccurredOn) : IDomainEvent;
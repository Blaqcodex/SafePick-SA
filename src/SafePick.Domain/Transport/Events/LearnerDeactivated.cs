using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a learner is removed from active transport.
/// The record is preserved for audit; the learner is no longer
/// expected at pickup.
/// </summary>
public sealed record LearnerDeactivated(
    Guid LearnerId,
    Guid SchoolId,
    DateTime OccurredOn) : IDomainEvent;
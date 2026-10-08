using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a learner's parent contact is set or replaced.
/// A learner cannot be assigned to a vehicle without one.
/// </summary>
public sealed record LearnerParentContactSet(
    Guid LearnerId,
    Guid SchoolId,
    string ParentEmail,
    DateTime OccurredOn) : IDomainEvent;
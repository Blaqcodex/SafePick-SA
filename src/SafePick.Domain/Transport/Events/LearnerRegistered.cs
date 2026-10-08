using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a learner is added to a school's roll.
/// </summary>
public sealed record LearnerRegistered(
    Guid LearnerId,
    Guid SchoolId,
    string FullName,
    string GradeClass,
    DateTime OccurredOn) : IDomainEvent;
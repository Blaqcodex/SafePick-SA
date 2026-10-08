using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a new school is created. This is the origin of a tenant.
/// Typically raised by the Operator when issuing the first admin invitation.
/// </summary>
public sealed record SchoolRegistered(
    Guid SchoolId,
    string Name,
    string ContactEmail,
    DateTime OccurredOn) : IDomainEvent;
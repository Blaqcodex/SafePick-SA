using SafePick.Domain.Common;

namespace SafePick.Domain.Identity.Events;

/// <summary>
/// Raised when the Operator account is created on first boot.
/// This is the origin of the authority chain.
/// </summary>
public sealed record OperatorSeeded(
    Guid UserId,
    string Email,
    DateTime OccurredOn) : IDomainEvent;
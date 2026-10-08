using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a school's transport window configuration changes.
/// Consumers (reconciliation, reporting) may need to react.
/// </summary>
public sealed record TransportWindowUpdated(
    Guid SchoolId,
    string WindowDescription,
    DateTime OccurredOn) : IDomainEvent;
using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a driver registers themselves and their vehicle.
/// A registered driver has no custody powers until approved by a school.
/// </summary>
public sealed record DriverRegistered(
    Guid DriverId,
    string FullName,
    string Email,
    string VehicleRegistration,
    DateTime OccurredOn) : IDomainEvent;
using MediatR;

namespace SafePick.Application.Transport.Drivers.Queries.GetCurrentDriver;

/// <summary>
/// Returns the authenticated driver's own details, including their
/// approval status for the school they registered with.
///
/// Used by the driver's landing dashboard to decide whether to
/// show the roster or the "awaiting approval" state.
/// </summary>
public sealed record GetCurrentDriverQuery()
    : IRequest<CurrentDriverDto>;

public sealed record CurrentDriverDto(
    Guid DriverId,
    string FullName,
    string Email,
    string Phone,
    string IdNumber,
    CurrentDriverVehicleDto Vehicle,
    string ApprovalStatus,
    Guid? SchoolId,
    string? SchoolName);

public sealed record CurrentDriverVehicleDto(
    Guid VehicleId,
    string Registration,
    string Make,
    string Model,
    int Capacity);
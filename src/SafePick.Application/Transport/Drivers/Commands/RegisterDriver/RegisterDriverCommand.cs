using MediatR;

namespace SafePick.Application.Transport.Drivers.Commands.RegisterDriver;

/// <summary>
/// Registers a new driver and their vehicle, creates a pending
/// approval for a chosen school, and creates a User account so
/// the driver can log in.
///
/// The driver has no custody powers until a school admin approves them.
/// </summary>
public sealed record RegisterDriverCommand(
    string FullName,
    string IdNumber,
    string Phone,
    string Email,
    string Password,
    string VehicleRegistrationNumber,
    string VehicleMake,
    string VehicleModel,
    int VehicleCapacity,
    Guid SchoolId) : IRequest<Guid>;
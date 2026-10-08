using MediatR;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Drivers.Queries.GetDriverApprovalsBySchool;

/// <summary>
/// Returns the driver approval records for a school, optionally
/// filtered by status. The school admin uses this to review
/// pending registrations and manage approved drivers.
/// </summary>
public sealed record GetDriverApprovalsBySchoolQuery(
    Guid SchoolId,
    DriverApproval.ApprovalStatus? Status = null)
    : IRequest<IReadOnlyList<DriverApprovalItemDto>>;

/// <summary>
/// One row of the driver approvals board.
/// </summary>
public sealed record DriverApprovalItemDto(
    Guid ApprovalId,
    Guid DriverId,
    string DriverFullName,
    string DriverEmail,
    string DriverPhone,
    VehicleDto Vehicle,
    Guid SchoolId,
    DriverApprovalStatusDto Status,
    DateTime RequestedAt,
    DateTime? DecidedAt,
    Guid? DecidedByUserId,
    string? Reason);

public sealed record VehicleDto(
    Guid VehicleId,
    string Registration,
    string Make,
    string Model,
    int Capacity);

/// <summary>
/// Mirrors the domain ApprovalStatus enum so the API surface is
/// decoupled from the domain model.
/// </summary>
public enum DriverApprovalStatusDto
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Revoked = 3
}
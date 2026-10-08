using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Transport.Drivers.Queries.GetCurrentDriver;

public sealed class GetCurrentDriverHandler
    : IRequestHandler<GetCurrentDriverQuery, CurrentDriverDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentDriverHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CurrentDriverDto> Handle(
        GetCurrentDriverQuery request,
        CancellationToken cancellationToken)
    {
        var email = _currentUser.Email
            ?? throw new ForbiddenAccessException("No authenticated user.");

        // Match the driver by email — the same mechanism AuthController
        // uses to resolve driverId at login time.
        var driver = await _context.Drivers
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.Email == email, cancellationToken)
            ?? throw new NotFoundException("Driver", email);

        // Find the driver's most recent approval record for their school.
        // A driver may have multiple approvals (one per school); we
        // return the first non-revoked one, preferring the most recent.
        var schoolId = _currentUser.SchoolId;

        var approval = await _context.DriverApprovals
            .Where(a => a.DriverId == driver.Id)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var approvalStatus = approval?.Status.ToString() ?? "Pending";
        var approvalSchoolId = approval?.SchoolId ?? schoolId;

        string? schoolName = null;
        if (approvalSchoolId is not null)
        {
            schoolName = await _context.Schools
                .Where(s => s.Id == approvalSchoolId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new CurrentDriverDto(
            DriverId: driver.Id,
            FullName: driver.FullName,
            Email: driver.Email,
            Phone: driver.Phone,
            IdNumber: driver.IdNumber,
            Vehicle: new CurrentDriverVehicleDto(
                VehicleId: driver.Vehicle.Id,
                Registration: driver.Vehicle.RegistrationNumber,
                Make: driver.Vehicle.Make,
                Model: driver.Vehicle.Model,
                Capacity: driver.Vehicle.Capacity),
            ApprovalStatus: approvalStatus,
            SchoolId: approvalSchoolId,
            SchoolName: schoolName);
    }
}
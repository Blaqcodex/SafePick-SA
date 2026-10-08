using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Identity;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Drivers.Commands.RegisterDriver;

public sealed class RegisterDriverHandler
    : IRequestHandler<RegisterDriverCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTime _clock;

    public RegisterDriverHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IDateTime clock)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        RegisterDriverCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var email = request.Email.Trim().ToLowerInvariant();

        // Reject if the email is already in use by a User.
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
            throw new DomainException(
                "An account with this email already exists. Please sign in instead.");

        // Reject if a Driver with the same email already exists.
        var driverExists = await _context.Drivers
            .AnyAsync(d => d.Email == email, cancellationToken);

        if (driverExists)
            throw new DomainException(
                "A driver with this email is already registered.");

        // 1. Create the Vehicle.
        var vehicle = Vehicle.Register(
            request.VehicleRegistrationNumber,
            request.VehicleMake,
            request.VehicleModel,
            request.VehicleCapacity);

        // 2. Create the Driver.
        var driver = Driver.Register(
            request.FullName,
            request.IdNumber,
            request.Phone,
            request.Email,
            vehicle,
            now);

        // 3. Create the pending approval for the chosen school.
        var approval = DriverApproval.Request(driver.Id, request.SchoolId, now);

        // 4. Create the User account so the driver can log in.
        //    The User and Driver are linked by matching email —
        //    AuthController resolves driverId by that match.
        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.CreateDriver(
            email: email,
            passwordHash: passwordHash,
            displayName: driver.FullName,
            schoolId: request.SchoolId);

        _context.Drivers.Add(driver);
        _context.DriverApprovals.Add(approval);
        _context.Users.Add(user);

        await _context.SaveChangesAsync(cancellationToken);

        return driver.Id;
    }
}
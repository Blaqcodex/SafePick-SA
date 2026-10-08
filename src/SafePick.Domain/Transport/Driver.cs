using SafePick.Domain.Common;
using SafePick.Domain.Transport.Events;

namespace SafePick.Domain.Transport;

/// <summary>
/// A transport provider — a person who drives learners.
///
/// Driver is a RICH aggregate root: it owns its Vehicle (exactly one).
/// The driver's registration is independent of any school; approval is
/// per-school and lives in a separate DriverApproval aggregate.
///
/// A self-registered Driver has NO custody powers. Custody powers are
/// granted by DriverApproval (per school), not by Driver itself.
/// </summary>
public sealed class Driver : AggregateRoot<Guid>
{
    public string FullName { get; private set; }
    public string IdNumber { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }
    public Vehicle Vehicle { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core materialization constructor.
    // EF Core cannot bind constructor parameters that are owned types
    // (Vehicle). It uses this constructor to create an instance from a
    // row, then assigns properties via their private setters.
    private Driver() : base(Guid.Empty)
    {
        FullName = string.Empty;
        IdNumber = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Vehicle = null!;
        IsActive = false;
    }

    private Driver(
        Guid id,
        string fullName,
        string idNumber,
        string phone,
        string email,
        Vehicle vehicle) : base(id)
    {
        FullName = fullName;
        IdNumber = idNumber;
        Phone = phone;
        Email = email;
        Vehicle = vehicle;
        IsActive = true;
    }

    /// <summary>
    /// Registers a new driver together with their vehicle.
    /// The driver exists in the system but has NO custody powers until
    /// approved by a school (see DriverApproval).
    /// </summary>
    public static Driver Register(
        string fullName,
        string idNumber,
        string phone,
        string email,
        Vehicle vehicle,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Driver full name must not be empty.");

        if (string.IsNullOrWhiteSpace(idNumber))
            throw new DomainException("Driver ID number must not be empty.");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Driver phone must not be empty.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Driver email must not be empty.");

        ArgumentNullException.ThrowIfNull(vehicle);

        var driver = new Driver(
            Guid.NewGuid(),
            fullName.Trim(),
            idNumber.Trim(),
            phone.Trim(),
            email.Trim().ToLowerInvariant(),
            vehicle);

        driver.Raise(new DriverRegistered(
            driver.Id,
            driver.FullName,
            driver.Email,
            driver.Vehicle.RegistrationNumber,
            now));

        return driver;
    }

    /// <summary>
    /// Updates the driver's personal details and vehicle.
    /// Per the executive summary, this is subject to re-approval — the
    /// Application layer will handle the re-approval workflow.
    /// </summary>
    public void UpdateDetails(
        string fullName,
        string phone,
        string email,
        string registrationNumber,
        string make,
        string model,
        int capacity)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Driver full name must not be empty.");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Driver phone must not be empty.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Driver email must not be empty.");

        FullName = fullName.Trim();
        Phone = phone.Trim();
        Email = email.Trim().ToLowerInvariant();
        Vehicle.UpdateDetails(registrationNumber, make, model, capacity);
    }

    /// <summary>
    /// Deactivates the driver. The record is preserved for audit.
    /// Any DriverApproval rows must be handled by the Application layer.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Driver is already inactive.");

        IsActive = false;
    }
}
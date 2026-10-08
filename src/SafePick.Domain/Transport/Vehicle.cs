using SafePick.Domain.Common;

namespace SafePick.Domain.Transport;

/// <summary>
/// A transport vehicle registered by a driver.
///
/// This is an ENTITY, not an aggregate root — it has identity, but no
/// independent lifecycle. A Vehicle is always owned by a Driver, and is
/// created and destroyed with that Driver. Only the Driver aggregate
/// raises domain events.
/// </summary>
public sealed class Vehicle : Entity<Guid>
{
    public string RegistrationNumber { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Capacity { get; private set; }

    private Vehicle(
        Guid id,
        string registrationNumber,
        string make,
        string model,
        int capacity) : base(id)
    {
        RegistrationNumber = registrationNumber;
        Make = make;
        Model = model;
        Capacity = capacity;
    }

    /// <summary>
    /// Creates a new vehicle. Enforces the capacity invariant: a vehicle
    /// must have a positive, sane learner capacity.
    /// </summary>
    public static Vehicle Register(
        string registrationNumber,
        string make,
        string model,
        int capacity)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new DomainException("Vehicle registration number must not be empty.");

        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Vehicle make must not be empty.");

        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Vehicle model must not be empty.");

        if (capacity <= 0)
            throw new DomainException("Vehicle capacity must be positive.");

        if (capacity > 100)
            throw new DomainException("Vehicle capacity exceeds a sane maximum (100).");

        return new Vehicle(
            Guid.NewGuid(),
            registrationNumber.Trim().ToUpperInvariant(),
            make.Trim(),
            model.Trim(),
            capacity);
    }

    /// <summary>
    /// Updates the vehicle's mutable details. Capacity can be lowered but
    /// never below the number of currently assigned learners — that
    /// cross-aggregate check happens in the Application layer.
    /// </summary>
    public void UpdateDetails(string registrationNumber, string make, string model, int capacity)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new DomainException("Vehicle registration number must not be empty.");

        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Vehicle make must not be empty.");

        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Vehicle model must not be empty.");

        if (capacity <= 0)
            throw new DomainException("Vehicle capacity must be positive.");

        if (capacity > 100)
            throw new DomainException("Vehicle capacity exceeds a sane maximum (100).");

        RegistrationNumber = registrationNumber.Trim().ToUpperInvariant();
        Make = make.Trim();
        Model = model.Trim();
        Capacity = capacity;
    }
}
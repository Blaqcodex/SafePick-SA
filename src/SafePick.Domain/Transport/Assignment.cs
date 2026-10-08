using SafePick.Domain.Common;
using SafePick.Domain.Transport.Events;

namespace SafePick.Domain.Transport;

/// <summary>
/// The assignment of a learner to a vehicle (and thus to a driver) for a
/// period of time.
///
/// Assignment is a thin aggregate root, scoped to a SchoolId (ADR-0003).
/// Its lifecycle is temporal: it is valid during [ValidFrom, ValidTo).
/// A null ValidTo means "still active, no end date set."
///
/// The invariant "a learner cannot have two overlapping assignments" is
/// enforced in the domain by the Application layer and, critically, in
/// the database by a PostgreSQL EXCLUDE constraint (see the
/// AssignmentConfiguration). The database constraint makes violation
/// *impossible*, not just *checked*.
/// </summary>
public sealed class Assignment : AggregateRoot<Guid>
{
    public Guid SchoolId { get; private set; }
    public Guid LearnerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public Guid DriverId { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public Guid AssignedByUserId { get; private set; }

    // EF Core materialization constructor.
    private Assignment() : base(Guid.Empty)
    {
    }

    private Assignment(
        Guid id,
        Guid schoolId,
        Guid learnerId,
        Guid vehicleId,
        Guid driverId,
        DateTime validFrom,
        Guid assignedByUserId) : base(id)
    {
        SchoolId = schoolId;
        LearnerId = learnerId;
        VehicleId = vehicleId;
        DriverId = driverId;
        ValidFrom = validFrom;
        AssignedByUserId = assignedByUserId;
    }

    /// <summary>
    /// Creates a new assignment. The Application layer is responsible for
    /// verifying that the learner has no other active assignment, that
    /// the vehicle has capacity, and that both belong to the same school.
    /// </summary>
    public static Assignment Create(
        Guid schoolId,
        Guid learnerId,
        Guid vehicleId,
        Guid driverId,
        DateTime validFrom,
        Guid assignedByUserId,
        DateTime now)
    {
        if (schoolId == Guid.Empty)
            throw new DomainException("SchoolId is required.");

        if (learnerId == Guid.Empty)
            throw new DomainException("LearnerId is required.");

        if (vehicleId == Guid.Empty)
            throw new DomainException("VehicleId is required.");

        if (driverId == Guid.Empty)
            throw new DomainException("DriverId is required.");

        if (assignedByUserId == Guid.Empty)
            throw new DomainException("AssignedByUserId is required.");

        var assignment = new Assignment(
            Guid.NewGuid(),
            schoolId,
            learnerId,
            vehicleId,
            driverId,
            validFrom,
            assignedByUserId);

        assignment.Raise(new LearnerAssignedToVehicle(
            assignment.Id,
            assignment.SchoolId,
            assignment.LearnerId,
            assignment.VehicleId,
            assignment.DriverId,
            assignment.ValidFrom,
            assignment.AssignedByUserId,
            now));

        return assignment;
    }

    /// <summary>
    /// Whether this assignment is active at a given moment.
    /// </summary>
    public bool IsActiveAt(DateTime moment)
        => ValidFrom <= moment && (ValidTo is null || moment < ValidTo.Value);

    /// <summary>
    /// Terminates the assignment at a given moment. Only valid while the
    /// assignment is still open (ValidTo is null). Raises an event.
    /// </summary>
    public void Terminate(DateTime terminatedAt, Guid unassignedByUserId, DateTime now)
    {
        if (ValidTo is not null)
            throw new DomainException("Assignment has already been terminated.");

        if (terminatedAt < ValidFrom)
            throw new DomainException("Termination date cannot precede the assignment's start.");

        if (unassignedByUserId == Guid.Empty)
            throw new DomainException("UnassignedByUserId is required.");

        ValidTo = terminatedAt;

        Raise(new LearnerUnassignedFromVehicle(
            Id,
            SchoolId,
            LearnerId,
            VehicleId,
            terminatedAt,
            unassignedByUserId,
            now));
    }

    /// <summary>
    /// Whether this assignment is currently open (no termination date).
    /// </summary>
    public bool IsOpen => ValidTo is null;
}
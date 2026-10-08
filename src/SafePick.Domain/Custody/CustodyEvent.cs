using SafePick.Domain.Common;
using SafePick.Domain.Custody.Events;

namespace SafePick.Domain.Custody;

/// <summary>
/// An immutable record that a learner's custody transitioned.
///
/// This is the atomic unit of truth in SafePick SA. Every notification,
/// every alert, every receipt, and every audit trail entry in the system
/// is a *derived consequence* of custody events. The event is the input;
/// everything else is reaction.
///
/// Immutability is absolute:
///   - There are no setters (public or private) after construction.
///   - There are no mutating methods.
///   - The only construction paths are the four static factories below.
///
/// Immutability is enforced *again* at the database layer (ADR-0004):
/// the migration revokes UPDATE and DELETE grants on this table. Even a
/// bug in the application cannot rewrite a custody event.
///
/// Corrections do not modify an event. They are new events that reference
/// the original by id (see CustodyCorrectionRecorded).
/// </summary>
public sealed class CustodyEvent : AggregateRoot<Guid>
{
    // ---- Identity and scope -------------------------------------------------

    public CustodyEventType Type { get; private set; }
    public Guid SchoolId { get; private set; }
    public Guid LearnerId { get; private set; }

    // ---- Parties of record --------------------------------------------------

    public Guid? DriverId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public Guid? AssignmentId { get; private set; }
    public Guid? MarkedByUserId { get; private set; }

    // ---- Idempotency and forensics -----------------------------------------

    /// <summary>
    /// Client-generated identifier. If a driver's device retries a
    /// submission (offline sync, double tap, network timeout), the second
    /// insert is rejected by a unique constraint on this column rather than
    /// producing a duplicate event.
    /// </summary>
    public Guid ClientGeneratedId { get; private set; }

    /// <summary>
    /// The authoritative instant the transition occurred, as recorded by
    /// the server (ADR-0012). All reconciliation, ordering, and legal
    /// records use this value.
    /// </summary>
    public ServerTimestamp ServerTimestamp { get; private set; }

    /// <summary>
    /// The instant as reported by the driver's device. Informational only:
    /// used for diagnosing offline sync windows, never for ordering or
    /// for establishing custody facts.
    /// </summary>
    public DateTime? DeviceTimestamp { get; private set; }

    // ---- Event-specific payloads -------------------------------------------

    /// <summary>Populated only for NotInCar events.</summary>
    public string? Reason { get; private set; }

    /// <summary>Populated only for Correction events.</summary>
    public Guid? OriginalEventId { get; private set; }

    /// <summary>Populated only for SchoolMarkedAbsent events.</summary>
    public string? Note { get; private set; }

    // ---- EF Core materialization -------------------------------------------

    private CustodyEvent() : base(Guid.Empty)
    {
        ServerTimestamp = null!;
    }

    // ---- Private canonical constructor -------------------------------------

    private CustodyEvent(
        Guid id,
        CustodyEventType type,
        Guid schoolId,
        Guid learnerId,
        Guid? driverId,
        Guid? vehicleId,
        Guid? assignmentId,
        Guid? markedByUserId,
        Guid clientGeneratedId,
        ServerTimestamp serverTimestamp,
        DateTime? deviceTimestamp,
        string? reason,
        Guid? originalEventId,
        string? note) : base(id)
    {
        Type = type;
        SchoolId = schoolId;
        LearnerId = learnerId;
        DriverId = driverId;
        VehicleId = vehicleId;
        AssignmentId = assignmentId;
        MarkedByUserId = markedByUserId;
        ClientGeneratedId = clientGeneratedId;
        ServerTimestamp = serverTimestamp;
        DeviceTimestamp = deviceTimestamp;
        Reason = reason;
        OriginalEventId = originalEventId;
        Note = note;
    }

    // ---- Static factories (the only way to create an event) ---------------

    /// <summary>
    /// Records a confirmed pickup.
    /// </summary>
    public static CustodyEvent RecordPickup(
        Guid schoolId,
        Guid learnerId,
        Guid driverId,
        Guid vehicleId,
        Guid assignmentId,
        Guid clientGeneratedId,
        ServerTimestamp serverTimestamp,
        DateTime? deviceTimestamp,
        DateTime now)
    {
        RequireNonEmpty(schoolId, nameof(schoolId));
        RequireNonEmpty(learnerId, nameof(learnerId));
        RequireNonEmpty(driverId, nameof(driverId));
        RequireNonEmpty(vehicleId, nameof(vehicleId));
        RequireNonEmpty(assignmentId, nameof(assignmentId));
        RequireNonEmpty(clientGeneratedId, nameof(clientGeneratedId));
        ArgumentNullException.ThrowIfNull(serverTimestamp);

        var evt = new CustodyEvent(
            Guid.NewGuid(),
            CustodyEventType.PickedUp,
            schoolId,
            learnerId,
            driverId,
            vehicleId,
            assignmentId,
            markedByUserId: null,
            clientGeneratedId,
            serverTimestamp,
            deviceTimestamp,
            reason: null,
            originalEventId: null,
            note: null);

        evt.Raise(new LearnerPickedUp(
            evt.Id, schoolId, learnerId, driverId, vehicleId, assignmentId,
            clientGeneratedId, serverTimestamp.Value, deviceTimestamp, now));

        return evt;
    }

    /// <summary>
    /// Records that a learner was expected but not collected, with a reason.
    /// </summary>
    public static CustodyEvent RecordNotInCar(
        Guid schoolId,
        Guid learnerId,
        Guid driverId,
        Guid vehicleId,
        Guid assignmentId,
        Guid clientGeneratedId,
        string reason,
        ServerTimestamp serverTimestamp,
        DateTime? deviceTimestamp,
        DateTime now)
    {
        RequireNonEmpty(schoolId, nameof(schoolId));
        RequireNonEmpty(learnerId, nameof(learnerId));
        RequireNonEmpty(driverId, nameof(driverId));
        RequireNonEmpty(vehicleId, nameof(vehicleId));
        RequireNonEmpty(assignmentId, nameof(assignmentId));
        RequireNonEmpty(clientGeneratedId, nameof(clientGeneratedId));
        ArgumentNullException.ThrowIfNull(serverTimestamp);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(
                "A reason is required when marking a learner as not in the car.");

        var evt = new CustodyEvent(
            Guid.NewGuid(),
            CustodyEventType.NotInCar,
            schoolId,
            learnerId,
            driverId,
            vehicleId,
            assignmentId,
            markedByUserId: null,
            clientGeneratedId,
            serverTimestamp,
            deviceTimestamp,
            reason.Trim(),
            originalEventId: null,
            note: null);

        evt.Raise(new LearnerNotInCar(
            evt.Id, schoolId, learnerId, driverId, vehicleId, assignmentId,
            clientGeneratedId, reason.Trim(), serverTimestamp.Value,
            deviceTimestamp, now));

        return evt;
    }

    /// <summary>
    /// Records that the school marked a learner as absent before the window.
    /// </summary>
    public static CustodyEvent RecordSchoolAbsent(
        Guid schoolId,
        Guid learnerId,
        Guid markedByUserId,
        string? note,
        Guid clientGeneratedId,
        ServerTimestamp serverTimestamp,
        DateTime now)
    {
        RequireNonEmpty(schoolId, nameof(schoolId));
        RequireNonEmpty(learnerId, nameof(learnerId));
        RequireNonEmpty(markedByUserId, nameof(markedByUserId));
        RequireNonEmpty(clientGeneratedId, nameof(clientGeneratedId));
        ArgumentNullException.ThrowIfNull(serverTimestamp);

        var evt = new CustodyEvent(
            Guid.NewGuid(),
            CustodyEventType.SchoolMarkedAbsent,
            schoolId,
            learnerId,
            driverId: null,
            vehicleId: null,
            assignmentId: null,
            markedByUserId,
            clientGeneratedId,
            serverTimestamp,
            deviceTimestamp: null,
            reason: null,
            originalEventId: null,
            note: string.IsNullOrWhiteSpace(note) ? null : note.Trim());

        evt.Raise(new SchoolMarkedAbsent(
            evt.Id, schoolId, learnerId, markedByUserId,
            evt.Note, serverTimestamp.Value, now));

        return evt;
    }

    /// <summary>
    /// Records a correction to a prior custody event. The original event
    /// is never modified — this is a new event that references it.
    /// </summary>
    public static CustodyEvent RecordCorrection(
        Guid originalEventId,
        Guid schoolId,
        Guid learnerId,
        Guid correctedByUserId,
        string reason,
        Guid clientGeneratedId,
        ServerTimestamp serverTimestamp,
        DateTime now)
    {
        RequireNonEmpty(originalEventId, nameof(originalEventId));
        RequireNonEmpty(schoolId, nameof(schoolId));
        RequireNonEmpty(learnerId, nameof(learnerId));
        RequireNonEmpty(correctedByUserId, nameof(correctedByUserId));
        RequireNonEmpty(clientGeneratedId, nameof(clientGeneratedId));
        ArgumentNullException.ThrowIfNull(serverTimestamp);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(
                "A correction requires a reason explaining what was wrong.");

        var evt = new CustodyEvent(
            Guid.NewGuid(),
            CustodyEventType.Correction,
            schoolId,
            learnerId,
            driverId: null,
            vehicleId: null,
            assignmentId: null,
            markedByUserId: correctedByUserId,
            clientGeneratedId,
            serverTimestamp,
            deviceTimestamp: null,
            reason.Trim(),
            originalEventId,
            note: null);

        evt.Raise(new CustodyCorrectionRecorded(
            evt.Id, originalEventId, schoolId, learnerId, correctedByUserId,
            reason.Trim(), serverTimestamp.Value, now));

        return evt;
    }

    // ---- Helpers -----------------------------------------------------------

    private static void RequireNonEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{name} is required.");
    }
}
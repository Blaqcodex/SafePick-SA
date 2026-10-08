using SafePick.Domain.Common;
using SafePick.Domain.Transport;

namespace SafePick.Domain.Custody;

/// <summary>
/// A *derived* statement that a custody transition was expected for a
/// particular learner, on a particular school day, within a particular
/// transport window.
///
/// This is NOT an entity. It has no identity and is never persisted.
/// Expectations are *computed* from source truth:
///
///   1. an active Assignment (learner → vehicle)
///   2. a valid school day (not a holiday, not a weekend)
///   3. an applicable TransportWindow
///   4. the learner is not marked absent
///
/// ADR-0009 in code: storing expectations would create a second source
/// of truth that rots as assignments change. Deriving them means they
/// are always consistent with the current assignments and always
/// recomputable for audit.
///
/// TIME ZONE: the transport window's Times are interpreted as
/// **South African Standard Time (SAST, UTC+2)** — the operating time
/// zone of every school in this system. The conversion to UTC is
/// explicit and does NOT depend on the server's local time zone.
///
/// This is deliberate. Without it, "06:00" would mean "06:00 wherever
/// the server happens to run", which is not a useful semantic.
/// </summary>
public sealed class CustodyExpectation : ValueObject
{
    // ---------- SAST time zone ----------
    //
    // SAST has no daylight saving time — it is a fixed UTC+2 offset.
    // We resolve the zone ID with a fallback chain so this works on
    // Windows ("South Africa Standard Time") and Linux/macOS
    // ("Africa/Johannesburg"). If neither is available, we fall back to
    // a fixed +2 custom zone — which is exactly correct for SAST.
    private static readonly TimeZoneInfo SastZone = ResolveSastZone();

    private static TimeZoneInfo ResolveSastZone()
    {
        var ids = new[]
        {
            "South Africa Standard Time",  // Windows
            "Africa/Johannesburg"          // Linux / macOS / IANA
        };

        foreach (var id in ids)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        // Fixed +2 fallback — SAST has no DST, so this is exact.
        return TimeZoneInfo.CreateCustomTimeZone(
            id: "SAST-Fixed",
            baseUtcOffset: TimeSpan.FromHours(2),
            displayName: "South Africa Standard Time",
            standardDisplayName: "SAST");
    }

    private static DateTime ToUtc(DateOnly date, TimeOnly time)
    {
        // 1. Combine into a DateTime with Kind=Unspecified.
        var local = date.ToDateTime(time);

        // 2. Interpret it as SAST and convert to UTC.
        var utc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(local, DateTimeKind.Unspecified),
            SastZone);

        return utc; // Kind = Utc
    }

    // ---------- Properties ----------

    public Guid SchoolId { get; }
    public Guid LearnerId { get; }
    public Guid AssignmentId { get; }
    public Guid DriverId { get; }
    public Guid VehicleId { get; }
    public DateTime WindowStart { get; }
    public DateTime WindowEnd { get; }
    public DateTime ReconciliationAt { get; }

    private CustodyExpectation(
        Guid schoolId,
        Guid learnerId,
        Guid assignmentId,
        Guid driverId,
        Guid vehicleId,
        DateTime windowStart,
        DateTime windowEnd,
        DateTime reconciliationAt)
    {
        SchoolId = schoolId;
        LearnerId = learnerId;
        AssignmentId = assignmentId;
        DriverId = driverId;
        VehicleId = vehicleId;
        WindowStart = windowStart;
        WindowEnd = windowEnd;
        ReconciliationAt = reconciliationAt;
    }

    /// <summary>
    /// Derives an expectation from an active assignment and a transport
    /// window for a given date. The transport window's Times are
    /// interpreted as South African Standard Time.
    /// </summary>
    public static CustodyExpectation Derive(
        Assignment assignment,
        TransportWindow window,
        DateOnly schoolDay)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(window);

        if (!assignment.IsOpen)
            throw new DomainException(
                "Cannot derive an expectation from a closed assignment.");

        var windowStart = ToUtc(schoolDay, window.OpensAt);
        var windowEnd = ToUtc(schoolDay, window.ClosesAt);
        var reconciliationAt = windowEnd.Add(window.GracePeriod);

        return new CustodyExpectation(
            assignment.SchoolId,
            assignment.LearnerId,
            assignment.Id,
            assignment.DriverId,
            assignment.VehicleId,
            windowStart,
            windowEnd,
            reconciliationAt);
    }

    /// <summary>
    /// Whether a custody event at the given server timestamp would
    /// resolve this expectation.
    /// </summary>
    public bool IsResolvedBy(DateTime serverTimestamp)
        => serverTimestamp >= WindowStart && serverTimestamp <= ReconciliationAt;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SchoolId;
        yield return LearnerId;
        yield return AssignmentId;
        yield return WindowStart;
        yield return WindowEnd;
    }

    public override string ToString()
        => $"Expectation[{LearnerId} on {WindowStart:O} → {WindowEnd:O}]";
}
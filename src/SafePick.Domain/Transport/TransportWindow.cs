using SafePick.Domain.Common;

namespace SafePick.Domain.Transport;

/// <summary>
/// A daily transport window for a school — the period during which pickups
/// (or dropoffs) are expected to occur.
///
/// This is a value object: it has no identity, it is defined entirely by
/// its values, and it is immutable. A "change" to a window produces a new
/// window, never a mutation.
/// </summary>
public sealed class TransportWindow : ValueObject
{
    public TimeOnly OpensAt { get; }
    public TimeOnly ClosesAt { get; }
    public TimeSpan GracePeriod { get; }

    public TransportWindow(TimeOnly opensAt, TimeOnly closesAt, TimeSpan gracePeriod)
    {
        if (closesAt <= opensAt)
            throw new DomainException(
                "Transport window close time must be after open time.");

        if (gracePeriod < TimeSpan.Zero)
            throw new DomainException(
                "Transport window grace period cannot be negative.");

        OpensAt = opensAt;
        ClosesAt = closesAt;
        GracePeriod = gracePeriod;
    }

    /// <summary>
    /// The moment after which an unresolved expectation becomes a gap.
    /// </summary>
    public TimeOnly ReconciliationAt => ClosesAt.Add(GracePeriod);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OpensAt;
        yield return ClosesAt;
        yield return GracePeriod;
    }

    public override string ToString()
        => $"{OpensAt:HH\\:mm}–{ClosesAt:HH\\:mm} (grace {GracePeriod.TotalMinutes:0}m)";
}
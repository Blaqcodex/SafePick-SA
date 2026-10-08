using SafePick.Domain.Common;

namespace SafePick.Domain.Custody;

/// <summary>
/// An authoritative, time-zone-aware instant.
///
/// This is ADR-0012 in code: the domain never reads the system clock
/// directly. All timestamps flow through the application layer's IDateTime
/// abstraction and are wrapped here as a value object once they enter the
/// custody context.
///
/// ServerTimestamp always represents a UTC instant. Device timestamps
/// (recorded separately for forensics) are not represented by this type.
///
/// Comparison and equality operators are implemented so that EF Core's
/// value converter can translate queries like `e.ServerTimestamp >= threshold`
/// into SQL against the underlying server_timestamp column.
/// </summary>
public sealed class ServerTimestamp
    : ValueObject, IComparable<ServerTimestamp>
{
    public DateTime Value { get; }

    public ServerTimestamp(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException(
                "ServerTimestamp must be a UTC instant. Received: " + value.Kind);

        Value = value;
    }

    public static ServerTimestamp Now(DateTime utcNow) => new(utcNow);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // ---------- comparison ----------

    public int CompareTo(ServerTimestamp? other)
        => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator >=(ServerTimestamp left, ServerTimestamp right)
        => left.Value >= right.Value;

    public static bool operator <=(ServerTimestamp left, ServerTimestamp right)
        => left.Value <= right.Value;

    public static bool operator >(ServerTimestamp left, ServerTimestamp right)
        => left.Value > right.Value;

    public static bool operator <(ServerTimestamp left, ServerTimestamp right)
        => left.Value < right.Value;

    // ---------- equality (CA1036) ----------

    public static bool operator ==(ServerTimestamp? left, ServerTimestamp? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Value == right.Value;
    }

    public static bool operator !=(ServerTimestamp? left, ServerTimestamp? right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is ServerTimestamp other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value.ToString("O");
}
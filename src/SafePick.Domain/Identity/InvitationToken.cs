using SafePick.Domain.Common;

namespace SafePick.Domain.Identity;

/// <summary>
/// A single-use, time-bounded token embedded in an invitation link.
///
/// The raw token value is never stored in the domain — only its hash is
/// persisted (by the infrastructure). This value object carries the
/// token value (or its hash) during the invitation's creation and
/// during redemption.
///
/// Equality and comparison operators are implemented so that EF Core's
/// value converter can translate queries like `i.Token == hash` into
/// SQL against the underlying column. Without these, reaching through
/// `.Value` inside a LINQ Where clause fails to translate.
/// </summary>
public sealed class InvitationToken : ValueObject
{
    public string Value { get; }

    public InvitationToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Invitation token must not be empty.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // ---------- equality (required for EF Core translation) ----------

    public static bool operator ==(InvitationToken? left, InvitationToken? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Value == right.Value;
    }

    public static bool operator !=(InvitationToken? left, InvitationToken? right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is InvitationToken other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString() => Value;
}
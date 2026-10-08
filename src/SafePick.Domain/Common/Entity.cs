namespace SafePick.Domain.Common;

/// <summary>
/// Base class for domain entities.
///
/// An entity is defined by its identity: two entities of the same type are
/// equal if and only if their identifiers are equal, regardless of any other
/// attribute. Entities have a lifecycle and can change over time.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// The entity's identity. Immutable once set.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode()
        => (GetType().ToString() + Id).GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);
}
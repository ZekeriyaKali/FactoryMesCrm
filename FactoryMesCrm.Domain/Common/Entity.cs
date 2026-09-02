namespace FactoryMesCrm.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    public TId Id { get; private protected init; } = default!;

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default))
            throw new ArgumentException("Entity Id boş veya varsayılan değer olamaz.", nameof(id));

        Id = id;
    }

    protected Entity() { } // EF Core için

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id!);
    }
}
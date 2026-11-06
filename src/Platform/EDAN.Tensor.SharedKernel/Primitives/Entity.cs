namespace EDAN.Tensor.SharedKernel.Primitives;

/// <summary>Entity는 식별자를 기준으로 동일성을 비교하는 도메인 객체입니다.</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    protected Entity(TId id) { ArgumentNullException.ThrowIfNull(id); Id = id; }
    public TId Id { get; }
    public bool Equals(Entity<TId>? other) => other is not null && GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => EqualityComparer<Entity<TId>?>.Default.Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}

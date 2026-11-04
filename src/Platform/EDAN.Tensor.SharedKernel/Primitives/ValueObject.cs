namespace EDAN.Tensor.SharedKernel.Primitives;

/// <summary>Value-based equality for immutable domain values.</summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public bool Equals(ValueObject? other) => other is not null && GetType() == other.GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);
    public override int GetHashCode() { var hash = new HashCode(); hash.Add(GetType()); foreach (var component in GetEqualityComponents()) hash.Add(component); return hash.ToHashCode(); }
    public static bool operator ==(ValueObject? left, ValueObject? right) => EqualityComparer<ValueObject?>.Default.Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}

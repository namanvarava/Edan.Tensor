namespace EDAN.Tensor.SharedKernel.Primitives;

/// <summary>Type-safe identifier backed by a non-null primitive value.</summary>
public abstract record StronglyTypedId<TValue> where TValue : notnull
{
    protected StronglyTypedId(TValue value) { ArgumentNullException.ThrowIfNull(value); Value = value; }
    public TValue Value { get; }
}

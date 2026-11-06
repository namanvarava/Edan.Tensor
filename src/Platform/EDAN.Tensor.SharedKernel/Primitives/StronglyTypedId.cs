namespace EDAN.Tensor.SharedKernel.Primitives;

/// <summary>StronglyTypedId는 null이 아닌 기본 값으로 뒷받침되는 형식 안전 식별자입니다.</summary>
public abstract record StronglyTypedId<TValue> where TValue : notnull
{
    protected StronglyTypedId(TValue value) { ArgumentNullException.ThrowIfNull(value); Value = value; }
    public TValue Value { get; }
}

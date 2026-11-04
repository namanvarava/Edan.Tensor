using EDAN.Tensor.SharedKernel.Primitives;
using Xunit;
namespace EDAN.Tensor.SharedKernel.Tests;
public sealed class PrimitiveTests
{
    [Fact] public void Entities_WithSameTypeAndId_AreEqual() { var id = Guid.NewGuid(); var a = new SampleEntity(id); var b = new SampleEntity(id); Assert.Equal(a, b); Assert.Equal(a.GetHashCode(), b.GetHashCode()); }
    [Fact] public void ValueObjects_WithSameComponents_AreEqual() { Assert.Equal(new SampleValue("KRW", 25000), new SampleValue("KRW", 25000)); }
    [Fact] public void StronglyTypedId_ExposesUnderlyingValue() { var value = Guid.NewGuid(); Assert.Equal(value, new SampleId(value).Value); }
    private sealed class SampleEntity(Guid id) : Entity<Guid>(id) { }
    private sealed class SampleValue(string currency, long amount) : ValueObject { protected override IEnumerable<object?> GetEqualityComponents() { yield return currency; yield return amount; } }
    private sealed record SampleId(Guid IdValue) : StronglyTypedId<Guid>(IdValue);
}

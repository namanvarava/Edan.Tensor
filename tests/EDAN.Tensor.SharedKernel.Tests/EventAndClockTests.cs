using EDAN.Tensor.SharedKernel.Events;
using EDAN.Tensor.SharedKernel.Time;
using Xunit;
namespace EDAN.Tensor.SharedKernel.Tests;
public sealed class EventAndClockTests
{
    [Fact] public void IntegrationEvent_ContainsVersionAndCorrelationMetadata() { var id = Guid.NewGuid(); var correlation = Guid.NewGuid(); IIntegrationEvent value = new SampleEvent(id, DateTimeOffset.UtcNow, correlation); Assert.Equal(id, value.EventId); Assert.Equal("sample.created", value.EventType); Assert.Equal(1, value.SchemaVersion); Assert.Equal(correlation, value.CorrelationId); }
    [Fact] public void SystemClock_ReturnsUtcOffset() { Assert.Equal(TimeSpan.Zero, new SystemClock().UtcNow.Offset); }
    private sealed record SampleEvent(Guid Id, DateTimeOffset At, Guid? CorrelationIdValue) : IntegrationEvent(Id, At, CorrelationIdValue) { public override string EventType => "sample.created"; }
}

namespace EDAN.Tensor.SharedKernel.Time;
public sealed class SystemClock : ISystemClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>원본 DLQ 작업, 멱등성 요청 ID, 새 재처리 작업 및 요청자 이력입니다.</summary>
public sealed class WorkerTaskReplayRecord
{
    private WorkerTaskReplayRecord() { }

    public WorkerTaskReplayRecord(Guid tenantId, Guid originalTaskId, Guid replayTaskId,
        Guid replayRequestId, string requestedBy, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OriginalTaskId = originalTaskId;
        ReplayTaskId = replayTaskId;
        ReplayRequestId = replayRequestId;
        RequestedBy = requestedBy;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OriginalTaskId { get; private set; }
    public Guid ReplayTaskId { get; private set; }
    public Guid ReplayRequestId { get; private set; }
    public string RequestedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

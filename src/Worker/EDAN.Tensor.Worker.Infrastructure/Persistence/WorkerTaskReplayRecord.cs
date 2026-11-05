namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>원본 DLQ 작업과 새 재처리 작업의 연결 및 요청자 이력입니다.</summary>
public sealed class WorkerTaskReplayRecord
{
    private WorkerTaskReplayRecord() { }

    public WorkerTaskReplayRecord(Guid tenantId, Guid originalTaskId, Guid replayTaskId,
        string requestedBy, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OriginalTaskId = originalTaskId;
        ReplayTaskId = replayTaskId;
        RequestedBy = requestedBy;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OriginalTaskId { get; private set; }
    public Guid ReplayTaskId { get; private set; }
    public string RequestedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

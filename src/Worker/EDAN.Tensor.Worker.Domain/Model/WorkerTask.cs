namespace EDAN.Tensor.Worker.Domain.Model;

/// <summary>테넌트 범위의 비동기 작업과 실행 소유권을 관리합니다.</summary>
public sealed class WorkerTask
{
    private WorkerTask() { }

    private WorkerTask(Guid id, Guid tenantId, string taskType, string idempotencyKey,
        string payloadReference, DateTimeOffset nowUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("작업 식별자는 비어 있을 수 없습니다.", nameof(id));
        Id = id;
        TenantId = tenantId;
        TaskType = ValidateText(taskType, 120, nameof(taskType));
        IdempotencyKey = ValidateText(idempotencyKey, 200, nameof(idempotencyKey));
        PayloadReference = ValidateText(payloadReference, 500, nameof(payloadReference));
        CreatedAtUtc = nowUtc.ToUniversalTime();
        Status = WorkerTaskStatus.Pending;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string TaskType { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public string PayloadReference { get; private set; } = null!;
    public WorkerTaskStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 5;
    public Guid? LeaseId { get; private set; }
    public string? WorkerId { get; private set; }
    public long FencingToken { get; private set; }
    public DateTimeOffset? LeaseExpiresAtUtc { get; private set; }
    public string? Checkpoint { get; private set; }
    public long CheckpointVersion { get; private set; }
    public int ProgressPercent { get; private set; }
    public string? ProgressStage { get; private set; }
    public string? ProgressMessage { get; private set; }
    public string? ResultReference { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public string? LastErrorCode { get; private set; }
    public long Version { get; private set; }

    /// <summary>새 작업을 생성합니다.</summary>
    public static WorkerTask Create(Guid tenantId, string taskType, string idempotencyKey,
        string payloadReference, DateTimeOffset nowUtc, int maxAttempts = 5)
    {
        if (maxAttempts is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "최대 시도 횟수는 1~100이어야 합니다.");
        var task = new WorkerTask(Guid.NewGuid(), tenantId, taskType, idempotencyKey,
            payloadReference, nowUtc);
        task.MaxAttempts = maxAttempts;
        return task;
    }

    /// <summary>대기 또는 재시도 작업의 실행 임대를 획득하고 새 Fencing Token을 발급합니다.</summary>
    public long AcquireLease(string workerId, Guid leaseId, DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc)
    {
        if (Status is not (WorkerTaskStatus.Pending or WorkerTaskStatus.RetryScheduled or WorkerTaskStatus.Checkpointed))
            throw new InvalidOperationException("실행 가능한 상태의 작업만 임대를 획득할 수 있습니다.");
        if (NextAttemptAtUtc.HasValue && NextAttemptAtUtc.Value > nowUtc)
            throw new InvalidOperationException("재시도 예약 시각이 아직 도래하지 않았습니다.");
        ValidateLease(workerId, leaseId, nowUtc, expiresAtUtc);
        WorkerId = ValidateText(workerId, 200, nameof(workerId));
        LeaseId = leaseId;
        FencingToken++;
        LeaseExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        AttemptCount++;
        NextAttemptAtUtc = null;
        Status = WorkerTaskStatus.Leased;
        Version++;
        return FencingToken;
    }

    /// <summary>획득한 임대의 작업 실행을 시작합니다.</summary>
    public void Start(Guid leaseId, long fencingToken, DateTimeOffset nowUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status != WorkerTaskStatus.Leased)
            throw new InvalidOperationException("임대 상태의 작업만 실행할 수 있습니다.");
        Status = WorkerTaskStatus.Running;
        Version++;
    }

    /// <summary>현재 임대 소유권으로 체크포인트를 저장합니다.</summary>
    public void SaveCheckpoint(Guid leaseId, long fencingToken, string checkpoint, DateTimeOffset nowUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status is not (WorkerTaskStatus.Leased or WorkerTaskStatus.Running or WorkerTaskStatus.Checkpointed))
            throw new InvalidOperationException("실행 중인 작업만 체크포인트를 저장할 수 있습니다.");
        Checkpoint = ValidateText(checkpoint, 4000, nameof(checkpoint));
        CheckpointVersion++;
        Status = WorkerTaskStatus.Checkpointed;
        Version++;
    }

    /// <summary>현재 임대 소유권을 검증하고 Lease 만료 시각을 연장합니다.</summary>
    public void RenewLease(Guid leaseId, long fencingToken, DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (expiresAtUtc.ToUniversalTime() <= nowUtc.ToUniversalTime())
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "임대 만료 시각은 현재 시각보다 늦어야 합니다.");
        LeaseExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        Version++;
    }

    /// <summary>작업을 완료하고 실행 소유권을 해제합니다.</summary>
    public void ReportProgress(Guid leaseId, long fencingToken, int percentage, string stage, string? message, DateTimeOffset nowUtc)\n    {\n        EnsureLease(leaseId, fencingToken, nowUtc);\n        if (Status is not (WorkerTaskStatus.Running or WorkerTaskStatus.Checkpointed))\n            throw new InvalidOperationException("실행 중인 작업만 진행률을 보고할 수 있습니다.");\n        if (percentage is < 0 or > 100)\n            throw new ArgumentOutOfRangeException(nameof(percentage), "진행률은 0~100이어야 합니다.");\n        ProgressStage = ValidateText(stage, 120, nameof(stage));\n        if (message is not null && message.Length > 500)\n            throw new ArgumentException("진행 메시지는 500자를 초과할 수 없습니다.", nameof(message));\n        ProgressMessage = message?.Trim();\n        ProgressPercent = percentage;\n        Version++;\n    }\n\n    public void Complete(Guid leaseId, long fencingToken, DateTimeOffset nowUtc, string? resultReference = null)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status is not (WorkerTaskStatus.Running or WorkerTaskStatus.Checkpointed))
            throw new InvalidOperationException("실행 중인 작업만 완료할 수 있습니다.");
        Status = WorkerTaskStatus.Succeeded;
        CompletedAtUtc = nowUtc.ToUniversalTime();
        ClearLease();
        Version++;
    }

    /// <summary>재시도 가능한 실패를 예약하거나 최대 시도 횟수 초과 시 DLQ로 이동합니다.</summary>
    public void Fail(Guid leaseId, long fencingToken, string errorCode,
        DateTimeOffset nowUtc, DateTimeOffset retryAtUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status is not (WorkerTaskStatus.Leased or WorkerTaskStatus.Running or WorkerTaskStatus.Checkpointed))
            throw new InvalidOperationException("실행 중인 작업만 실패 처리할 수 있습니다.");
        var normalizedErrorCode = ValidateText(errorCode, 200, nameof(errorCode));
        if (AttemptCount < MaxAttempts && retryAtUtc.ToUniversalTime() <= nowUtc.ToUniversalTime())
            throw new ArgumentOutOfRangeException(nameof(retryAtUtc), "재시도 시각은 현재 시각보다 늦어야 합니다.");
        LastErrorCode = normalizedErrorCode;
        ClearLease();
        if (AttemptCount >= MaxAttempts)
        {
            Status = WorkerTaskStatus.DeadLettered;
            NextAttemptAtUtc = null;
        }
        else
        {
            Status = WorkerTaskStatus.RetryScheduled;
            NextAttemptAtUtc = retryAtUtc.ToUniversalTime();
        }
        Version++;
    }

    /// <summary>정상 종료 인계를 위해 체크포인트를 저장하고 작업을 대기 상태로 돌립니다.</summary>
    public void Handoff(Guid leaseId, long fencingToken, string checkpoint, DateTimeOffset nowUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status is not (WorkerTaskStatus.Leased or WorkerTaskStatus.Running or WorkerTaskStatus.Checkpointed))
            throw new InvalidOperationException("인계 가능한 실행 상태가 아닙니다.");
        Checkpoint = ValidateText(checkpoint, 4000, nameof(checkpoint));
        CheckpointVersion++;
        ClearLease();
        Status = WorkerTaskStatus.Pending;
        NextAttemptAtUtc = nowUtc.ToUniversalTime();
        Version++;
    }

    /// <summary>현재 임대 소유권으로 작업을 취소합니다.</summary>
    public void Cancel(Guid leaseId, long fencingToken, DateTimeOffset nowUtc)
    {
        EnsureLease(leaseId, fencingToken, nowUtc);
        if (Status is WorkerTaskStatus.Succeeded or WorkerTaskStatus.DeadLettered or WorkerTaskStatus.Cancelled)
            throw new InvalidOperationException("종료된 작업은 취소할 수 없습니다.");
        ClearLease();
        Status = WorkerTaskStatus.Cancelled;
        NextAttemptAtUtc = null;
        Version++;
    }

    private void EnsureLease(Guid leaseId, long fencingToken, DateTimeOffset nowUtc)
    {
        if (LeaseId != leaseId || FencingToken != fencingToken || !LeaseExpiresAtUtc.HasValue
            || LeaseExpiresAtUtc.Value <= nowUtc.ToUniversalTime())
            throw new InvalidOperationException("Lease가 만료되었거나 Fencing Token이 일치하지 않습니다.");
    }

    private static void ValidateLease(string workerId, Guid leaseId, DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        if (leaseId == Guid.Empty) throw new ArgumentException("Lease 식별자는 비어 있을 수 없습니다.", nameof(leaseId));
        if (expiresAtUtc.ToUniversalTime() <= nowUtc.ToUniversalTime())
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "임대 만료 시각은 현재 시각보다 늦어야 합니다.");
    }

    private void ClearLease()
    {
        LeaseId = null;
        WorkerId = null;
        LeaseExpiresAtUtc = null;
    }

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}

/// <summary>비동기 작업의 영속 상태를 구분합니다.</summary>
public enum WorkerTaskStatus
{
    Pending,
    Leased,
    Running,
    Checkpointed,
    RetryScheduled,
    Succeeded,
    DeadLettered,
    Cancelled
}

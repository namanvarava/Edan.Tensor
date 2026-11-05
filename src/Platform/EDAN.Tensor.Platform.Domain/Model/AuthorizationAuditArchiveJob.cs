namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>감사 아카이브 작업의 상태와 실행 결과를 관리합니다.</summary>
public sealed class AuthorizationAuditArchiveJob
{
    private AuthorizationAuditArchiveJob() { }

    private AuthorizationAuditArchiveJob(Guid id, TenantId tenantId, DateTimeOffset fromUtc,
        DateTimeOffset toUtc, string requestedBy, DateTimeOffset createdAtUtc)
    {
        if (fromUtc > toUtc || toUtc - fromUtc > TimeSpan.FromDays(31))
            throw new ArgumentException("아카이브 작업 기간은 최대 31일이며 시작 시각은 종료 시각보다 늦을 수 없습니다.");
        Id = id;
        TenantId = tenantId;
        FromUtc = fromUtc.ToUniversalTime();
        ToUtc = toUtc.ToUniversalTime();
        RequestedBy = ValidateActor(requestedBy);
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        Status = AuthorizationAuditArchiveJobStatus.Pending;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public DateTimeOffset FromUtc { get; private set; }
    public DateTimeOffset ToUtc { get; private set; }
    public string RequestedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public AuthorizationAuditArchiveJobStatus Status { get; private set; }
    public Guid? ArchiveId { get; private set; }
    public int RecordCount { get; private set; }
    public string? Sha256 { get; private set; }
    public int? SizeBytes { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public int AttemptCount { get; private set; }
    public long Version { get; private set; }

    /// <summary>새 감사 아카이브 작업을 생성합니다.</summary>
    public static AuthorizationAuditArchiveJob Create(TenantId tenantId, DateTimeOffset fromUtc,
        DateTimeOffset toUtc, string requestedBy, DateTimeOffset nowUtc) =>
        new(Guid.NewGuid(), tenantId, fromUtc, toUtc, requestedBy, nowUtc);

    /// <summary>아카이브 생성 성공 결과를 작업에 반영합니다.</summary>
    public void Complete(Guid archiveId, int recordCount, string sha256, int sizeBytes, DateTimeOffset nowUtc)
    {
        if (Status != AuthorizationAuditArchiveJobStatus.Pending)
            throw new InvalidOperationException("대기 상태의 아카이브 작업만 완료할 수 있습니다.");
        if (archiveId == Guid.Empty || recordCount < 0 || sizeBytes < 0 || sha256.Length != 64)
            throw new ArgumentException("아카이브 결과 정보가 올바르지 않습니다.");
        ArchiveId = archiveId;
        RecordCount = recordCount;
        Sha256 = sha256;
        SizeBytes = sizeBytes;
        CompletedAtUtc = nowUtc.ToUniversalTime();
        FailureReason = null;
        Status = AuthorizationAuditArchiveJobStatus.Completed;
        Version++;
    }

    /// <summary>아카이브 생성 실패 사유를 기록합니다.</summary>
    public void Fail(string reason)
    {
        if (Status != AuthorizationAuditArchiveJobStatus.Pending)
            throw new InvalidOperationException("대기 상태의 아카이브 작업만 실패 처리할 수 있습니다.");
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "archive.unexpected_failure" : reason.Trim()[..Math.Min(reason.Trim().Length, 500)];
        Status = AuthorizationAuditArchiveJobStatus.Failed;
        Version++;
    }

    /// <summary>실패한 작업의 재시도를 준비합니다.</summary>
    public void PrepareRetry()
    {
        if (Status != AuthorizationAuditArchiveJobStatus.Failed)
            throw new InvalidOperationException("실패한 아카이브 작업만 재시도할 수 있습니다.");
        Status = AuthorizationAuditArchiveJobStatus.Pending;
        FailureReason = null;
        AttemptCount++;
        Version++;
    }

    /// <summary>아카이브 무결성 검증 성공을 기록합니다.</summary>
    public void MarkVerified()
    {
        if (Status != AuthorizationAuditArchiveJobStatus.Completed
            && Status != AuthorizationAuditArchiveJobStatus.Verified)
            throw new InvalidOperationException("완료된 아카이브 작업만 검증할 수 있습니다.");
        Status = AuthorizationAuditArchiveJobStatus.Verified;
        Version++;
    }

    private static string ValidateActor(string actor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        var value = actor.Trim();
        if (value.Length > 200) throw new ArgumentException("요청 주체 식별자는 200자를 초과할 수 없습니다.");
        return value;
    }
}

/// <summary>감사 아카이브 작업 상태를 구분합니다.</summary>
public enum AuthorizationAuditArchiveJobStatus
{
    Pending,
    Completed,
    Verified,
    Failed
}

/// <summary>아카이브 생성·재시도·검증 실행의 결과를 추가 전용으로 보관합니다.</summary>
public sealed class AuthorizationAuditArchiveAttempt
{
    private AuthorizationAuditArchiveAttempt() { }

    private AuthorizationAuditArchiveAttempt(Guid id, Guid jobId, TenantId tenantId, int attemptNumber,
        string operation, DateTimeOffset startedAtUtc)
    {
        Id = id;
        JobId = jobId;
        TenantId = tenantId;
        AttemptNumber = attemptNumber;
        Operation = operation;
        StartedAtUtc = startedAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public int AttemptNumber { get; private set; }
    public string Operation { get; private set; } = null!;
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? FinishedAtUtc { get; private set; }
    public bool? IsSuccessful { get; private set; }
    public string? ResultCode { get; private set; }

    /// <summary>아카이브 실행 시도 이력을 시작합니다.</summary>
    public static AuthorizationAuditArchiveAttempt Start(Guid jobId, TenantId tenantId,
        int attemptNumber, string operation, DateTimeOffset nowUtc) =>
        new(Guid.NewGuid(), jobId, tenantId, attemptNumber, operation, nowUtc);

    /// <summary>실행 시도의 성공 또는 실패 결과를 확정합니다.</summary>
    public void Finish(bool isSuccessful, string resultCode, DateTimeOffset nowUtc)
    {
        if (FinishedAtUtc.HasValue) throw new InvalidOperationException("완료된 실행 이력은 변경할 수 없습니다.");
        ArgumentException.ThrowIfNullOrWhiteSpace(resultCode);
        FinishedAtUtc = nowUtc.ToUniversalTime();
        IsSuccessful = isSuccessful;
        ResultCode = resultCode.Trim()[..Math.Min(resultCode.Trim().Length, 200)];
    }
}

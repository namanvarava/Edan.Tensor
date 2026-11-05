namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>테넌트별 감사 기록 보존 기간과 법적 보존 상태를 관리합니다.</summary>
public sealed class AuthorizationAuditRetentionPolicy
{
    private AuthorizationAuditRetentionPolicy() { }

    private AuthorizationAuditRetentionPolicy(Guid id, TenantId tenantId, int retentionDays,
        bool isLegalHold, string? legalHoldReason, string updatedBy, DateTimeOffset updatedAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        RetentionDays = ValidateRetentionDays(retentionDays);
        IsLegalHold = isLegalHold;
        LegalHoldReason = ValidateHold(isLegalHold, legalHoldReason);
        UpdatedBy = ValidateActor(updatedBy);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        Version = 1;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public int RetentionDays { get; private set; }
    public bool IsLegalHold { get; private set; }
    public string? LegalHoldReason { get; private set; }
    public string UpdatedBy { get; private set; } = null!;
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }

    /// <summary>기본 보존 정책을 생성합니다.</summary>
    public static AuthorizationAuditRetentionPolicy Create(TenantId tenantId, int retentionDays,
        bool isLegalHold, string? legalHoldReason, string actor, DateTimeOffset nowUtc) =>
        new(Guid.NewGuid(), tenantId, retentionDays, isLegalHold, legalHoldReason, actor, nowUtc);

    /// <summary>보존 기간과 법적 보존 상태를 함께 변경합니다.</summary>
    public void Update(int retentionDays, bool isLegalHold, string? legalHoldReason,
        string actor, DateTimeOffset nowUtc)
    {
        RetentionDays = ValidateRetentionDays(retentionDays);
        IsLegalHold = isLegalHold;
        LegalHoldReason = ValidateHold(isLegalHold, legalHoldReason);
        UpdatedBy = ValidateActor(actor);
        UpdatedAtUtc = nowUtc.ToUniversalTime();
        Version++;
    }

    private static int ValidateRetentionDays(int days) =>
        days is >= 30 and <= 3650 ? days : throw new ArgumentOutOfRangeException(
            nameof(days), "감사 기록 보존 기간은 30일부터 3,650일까지 설정할 수 있습니다.");

    private static string? ValidateHold(bool enabled, string? reason)
    {
        if (!enabled) return null;
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        var value = reason.Trim();
        if (value.Length > 500)
            throw new ArgumentException("법적 보존 사유는 500자를 초과할 수 없습니다.");
        return value;
    }

    private static string ValidateActor(string actor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        var value = actor.Trim();
        if (value.Length > 200)
            throw new ArgumentException("변경 주체 식별자는 200자를 초과할 수 없습니다.");
        return value;
    }
}

/// <summary>감사 보존 정책의 변경 전후 상태를 보존하는 추가 전용 이력입니다.</summary>
public sealed class AuthorizationAuditRetentionChange
{
    private AuthorizationAuditRetentionChange() { }

    private AuthorizationAuditRetentionChange(Guid id, Guid policyId, TenantId tenantId,
        string actorSubjectId, string changeType, string beforeJson, string afterJson,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        PolicyId = policyId;
        TenantId = tenantId;
        ActorSubjectId = actorSubjectId;
        ChangeType = changeType;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid PolicyId { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public string ActorSubjectId { get; private set; } = null!;
    public string ChangeType { get; private set; } = null!;
    public string BeforeJson { get; private set; } = null!;
    public string AfterJson { get; private set; } = null!;
    public DateTimeOffset OccurredAtUtc { get; private set; }

    /// <summary>보존 정책 변경의 불변 감사 이력을 생성합니다.</summary>
    public static AuthorizationAuditRetentionChange Create(Guid policyId, TenantId tenantId,
        string actor, string changeType, string beforeJson, string afterJson, DateTimeOffset occurredAtUtc) =>
        new(Guid.NewGuid(), policyId, tenantId, actor, changeType, beforeJson, afterJson, occurredAtUtc);
}

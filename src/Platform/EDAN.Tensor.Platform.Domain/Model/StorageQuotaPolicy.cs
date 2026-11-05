namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>플랫폼 저장 용량 정책의 적용 범위입니다.</summary>
public enum StorageQuotaPolicyScope { Tenant, Organization, Project, DataSet }

/// <summary>Tenant 계층에 귀속되는 저장 용량 정책입니다.</summary>
public sealed class StorageQuotaPolicy
{
    private StorageQuotaPolicy() { }

    private StorageQuotaPolicy(Guid id, TenantId tenantId, StorageQuotaPolicyScope scope, Guid targetId,
        long? tenantLimitBytes, long? organizationLimitBytes, long? projectLimitBytes, long? dataSetLimitBytes,
        int warningThresholdPercent, string planCode, string updatedBy, DateTimeOffset updatedAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("정책 ID는 비어 있을 수 없습니다.", nameof(id));
        ArgumentNullException.ThrowIfNull(tenantId);
        if (!Enum.IsDefined(scope)) throw new ArgumentOutOfRangeException(nameof(scope));
        if (targetId == Guid.Empty) throw new ArgumentException("정책 대상 ID는 비어 있을 수 없습니다.", nameof(targetId));
        if (scope == StorageQuotaPolicyScope.Tenant && targetId != tenantId.Value)
            throw new ArgumentException("Tenant 정책 대상은 소유 Tenant와 일치해야 합니다.", nameof(targetId));
        ValidateLimits(tenantLimitBytes, organizationLimitBytes, projectLimitBytes, dataSetLimitBytes, warningThresholdPercent);
        Id = id; TenantId = tenantId; Scope = scope; TargetId = targetId;
        TenantLimitBytes = tenantLimitBytes; OrganizationLimitBytes = organizationLimitBytes;
        ProjectLimitBytes = projectLimitBytes; DataSetLimitBytes = dataSetLimitBytes;
        WarningThresholdPercent = warningThresholdPercent;
        PlanCode = Text(planCode, 64, nameof(planCode));
        UpdatedBy = Text(updatedBy, 200, nameof(updatedBy));
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        IsEnabled = true; Version = 1;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public StorageQuotaPolicyScope Scope { get; private set; }
    public Guid TargetId { get; private set; }
    public long? TenantLimitBytes { get; private set; }
    public long? OrganizationLimitBytes { get; private set; }
    public long? ProjectLimitBytes { get; private set; }
    public long? DataSetLimitBytes { get; private set; }
    public int WarningThresholdPercent { get; private set; }
    public string PlanCode { get; private set; } = "Community";
    public bool IsEnabled { get; private set; }
    public long Version { get; private set; }
    public string UpdatedBy { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static StorageQuotaPolicy Create(Guid id, TenantId tenantId, StorageQuotaPolicyScope scope, Guid targetId,
        long? tenantLimitBytes, long? organizationLimitBytes, long? projectLimitBytes, long? dataSetLimitBytes,
        int warningThresholdPercent, string planCode, string updatedBy, DateTimeOffset nowUtc) =>
        new(id, tenantId, scope, targetId, tenantLimitBytes, organizationLimitBytes, projectLimitBytes,
            dataSetLimitBytes, warningThresholdPercent, planCode, updatedBy, nowUtc);

    public void Update(long? tenantLimitBytes, long? organizationLimitBytes, long? projectLimitBytes,
        long? dataSetLimitBytes, int warningThresholdPercent, string planCode, string updatedBy,
        long expectedVersion, DateTimeOffset nowUtc)
    {
        CheckVersion(expectedVersion);
        ValidateLimits(tenantLimitBytes, organizationLimitBytes, projectLimitBytes, dataSetLimitBytes, warningThresholdPercent);
        TenantLimitBytes = tenantLimitBytes; OrganizationLimitBytes = organizationLimitBytes;
        ProjectLimitBytes = projectLimitBytes; DataSetLimitBytes = dataSetLimitBytes;
        WarningThresholdPercent = warningThresholdPercent;
        PlanCode = Text(planCode, 64, nameof(planCode));
        UpdatedBy = Text(updatedBy, 200, nameof(updatedBy));
        UpdatedAtUtc = nowUtc.ToUniversalTime(); Version++;
    }

    public void SetEnabled(bool enabled, string updatedBy, long expectedVersion, DateTimeOffset nowUtc)
    {
        CheckVersion(expectedVersion);
        UpdatedBy = Text(updatedBy, 200, nameof(updatedBy));
        if (IsEnabled == enabled) return;
        IsEnabled = enabled; UpdatedAtUtc = nowUtc.ToUniversalTime(); Version++;
    }

    private void CheckVersion(long expected)
    {
        if (expected != Version) throw new InvalidOperationException("Quota 정책 버전이 변경되었습니다. 최신 정책을 다시 조회해야 합니다.");
    }

    private static void ValidateLimits(long? tenant, long? organization, long? project, long? dataSet, int threshold)
    {
        if (tenant is < 0 || organization is < 0 || project is < 0 || dataSet is < 0)
            throw new ArgumentOutOfRangeException(nameof(tenant), "Quota 한도는 음수일 수 없습니다.");
        if (threshold is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(threshold), "경고 임계치는 1~100이어야 합니다.");
    }

    private static string Text(string value, int max, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var normalized = value.Trim();
        if (normalized.Length > max) throw new ArgumentException($"값은 {max}자를 초과할 수 없습니다.", name);
        return normalized;
    }
}

/// <summary>Quota 정책 변경 전후를 보존하는 감사 이벤트입니다.</summary>
public sealed class StorageQuotaPolicyChangeAuditEntry
{
    private StorageQuotaPolicyChangeAuditEntry() { }
    private StorageQuotaPolicyChangeAuditEntry(Guid id, TenantId tenantId, Guid policyId, string actor,
        string changeType, string beforeJson, string afterJson, DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty || policyId == Guid.Empty) throw new ArgumentException("감사 식별자는 비어 있을 수 없습니다.");
        Id = id; TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        PolicyId = policyId; ActorSubjectId = Text(actor, 200); ChangeType = Text(changeType, 32);
        BeforeJson = Text(beforeJson, 4000); AfterJson = Text(afterJson, 4000);
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }
    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public Guid PolicyId { get; private set; }
    public string ActorSubjectId { get; private set; } = string.Empty;
    public string ChangeType { get; private set; } = string.Empty;
    public string BeforeJson { get; private set; } = string.Empty;
    public string AfterJson { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public static StorageQuotaPolicyChangeAuditEntry Create(Guid id, TenantId tenantId, Guid policyId,
        string actor, string changeType, string beforeJson, string afterJson, DateTimeOffset occurredAtUtc) =>
        new(id, tenantId, policyId, actor, changeType, beforeJson, afterJson, occurredAtUtc);
    private static string Text(string value, int max)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var result = value.Trim();
        if (result.Length > max) throw new ArgumentException($"값은 {max}자를 초과할 수 없습니다.");
        return result;
    }
}

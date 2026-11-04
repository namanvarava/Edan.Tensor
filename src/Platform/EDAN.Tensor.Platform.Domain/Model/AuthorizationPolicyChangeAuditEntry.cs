using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>접근 정책 변경 감사 기록의 고유 식별자입니다.</summary>
public sealed record AuthorizationPolicyChangeAuditEntryId : StronglyTypedId<Guid>
{
    public AuthorizationPolicyChangeAuditEntryId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("정책 변경 감사 식별자는 비어 있을 수 없습니다.") : value) { }
}

/// <summary>정책 변경자와 변경 전후 상태를 추가 전용으로 기록합니다.</summary>
public sealed class AuthorizationPolicyChangeAuditEntry : AggregateRoot<AuthorizationPolicyChangeAuditEntryId>
{
    private AuthorizationPolicyChangeAuditEntry(AuthorizationPolicyChangeAuditEntryId id, TenantId tenantId,
        Guid policyId, string actorSubjectId, string changeType, string beforeJson, string afterJson,
        DateTimeOffset occurredAtUtc) : base(id)
    {
        TenantId = tenantId;
        PolicyId = policyId;
        ActorSubjectId = Validate(actorSubjectId, 200, nameof(actorSubjectId));
        ChangeType = Validate(changeType, 32, nameof(changeType));
        BeforeJson = Validate(beforeJson, 4000, nameof(beforeJson));
        AfterJson = Validate(afterJson, 4000, nameof(afterJson));
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public TenantId TenantId { get; }
    public Guid PolicyId { get; }
    public string ActorSubjectId { get; }
    public string ChangeType { get; }
    public string BeforeJson { get; }
    public string AfterJson { get; }
    public DateTimeOffset OccurredAtUtc { get; }

    /// <summary>정책 변경의 주체와 변경 전후 상태를 감사 기록으로 생성합니다.</summary>
    public static AuthorizationPolicyChangeAuditEntry Create(AuthorizationPolicyChangeAuditEntryId id,
        TenantId tenantId, Guid policyId, string actorSubjectId, string changeType,
        string beforeJson, string afterJson, DateTimeOffset occurredAtUtc) =>
        new(id, tenantId, policyId, actorSubjectId, changeType, beforeJson, afterJson, occurredAtUtc);

    private static string Validate(string value, int maximum, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var normalized = value.Trim();
        if (normalized.Length > maximum) throw new ArgumentException($"값은 {maximum}자를 초과할 수 없습니다.", name);
        return normalized;
    }
}
using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>권한 평가 감사 기록의 고유 식별자입니다.</summary>
public sealed record AuthorizationAuditEntryId : StronglyTypedId<Guid>
{
    public AuthorizationAuditEntryId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("감사 기록 식별자는 비어 있을 수 없습니다.") : value) { }
}

/// <summary>접근 허용·거부 결과를 변경 불가능한 감사 기록으로 보관합니다.</summary>
public sealed class AuthorizationAuditEntry : AggregateRoot<AuthorizationAuditEntryId>
{
    private AuthorizationAuditEntry(AuthorizationAuditEntryId id, TenantId tenantId, string subjectId,
        string action, string resourceType, string scope, bool isAllowed, string reasonCode,
        string? matchedPolicyId, DateTimeOffset occurredAtUtc) : base(id)
    {
        TenantId = tenantId;
        SubjectId = subjectId;
        Action = action;
        ResourceType = resourceType;
        Scope = scope;
        IsAllowed = isAllowed;
        ReasonCode = reasonCode;
        MatchedPolicyId = matchedPolicyId;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public TenantId TenantId { get; }
    public string SubjectId { get; }
    public string Action { get; }
    public string ResourceType { get; }
    public string Scope { get; }
    public bool IsAllowed { get; }
    public string ReasonCode { get; }
    public string? MatchedPolicyId { get; }
    public DateTimeOffset OccurredAtUtc { get; }

    /// <summary>권한 평가 결과를 감사 기록으로 생성합니다.</summary>
    public static AuthorizationAuditEntry Create(AuthorizationAuditEntryId id, TenantId tenantId,
        string subjectId, string action, string resourceType, string scope, AuthorizationDecision decision,
        DateTimeOffset occurredAtUtc) =>
        new(id, tenantId, subjectId, action, resourceType, scope, decision.IsAllowed,
            decision.ReasonCode, decision.MatchedPolicyId, occurredAtUtc);
}

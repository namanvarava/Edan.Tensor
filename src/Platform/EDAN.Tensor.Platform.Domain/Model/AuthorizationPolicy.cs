using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>정책 효과는 명시적 허용 또는 거부로 구분합니다.</summary>
public enum AuthorizationPolicyEffect { Allow, Deny }

/// <summary>접근 정책의 고유 식별자입니다.</summary>
public sealed record AuthorizationPolicyId : StronglyTypedId<Guid>
{
    public AuthorizationPolicyId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("접근 정책 식별자는 비어 있을 수 없습니다.") : value) { }
}

/// <summary>접근 정책은 Tenant 및 선택적 하위 범위에 적용되는 주체·작업 규칙입니다.</summary>
public sealed class AuthorizationPolicy : AggregateRoot<AuthorizationPolicyId>
{
    private AuthorizationPolicy(AuthorizationPolicyId id, TenantId tenantId, OrganizationId? organizationId,
        ProjectId? projectId, string subjectId, string action, string resourceType,
        AuthorizationPolicyEffect effect, int priority, DateTimeOffset createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
        ProjectId = projectId;
        SubjectId = Validate(subjectId, 200, "주체 식별자");
        Action = Validate(action, 120, "작업 식별자").ToLowerInvariant();
        ResourceType = Validate(resourceType, 80, "자원 유형");
        if (projectId is not null && organizationId is null)
            throw new ArgumentException("프로젝트 정책에는 조직 범위가 필요합니다.");
        if (!Enum.IsDefined(effect)) throw new ArgumentOutOfRangeException(nameof(effect));
        if (priority is < 0 or > 10000) throw new ArgumentOutOfRangeException(nameof(priority), "정책 우선순위는 0~10000이어야 합니다.");
        Effect = effect;
        Priority = priority;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        IsEnabled = true;
    }

    public TenantId TenantId { get; }
    public OrganizationId? OrganizationId { get; }
    public ProjectId? ProjectId { get; }
    public string SubjectId { get; }
    public string Action { get; }
    public string ResourceType { get; }
    public AuthorizationPolicyEffect Effect { get; private set; }
    public int Priority { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>검증된 접근 정책을 생성합니다.</summary>
    public static AuthorizationPolicy Create(AuthorizationPolicyId id, TenantId tenantId,
        OrganizationId? organizationId, ProjectId? projectId, string subjectId, string action,
        string resourceType, AuthorizationPolicyEffect effect, int priority, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(tenantId);
        return new(id, tenantId, organizationId, projectId, subjectId, action, resourceType, effect, priority, createdAtUtc);
    }

    /// <summary>정책 효과와 우선순위를 검증한 뒤 변경합니다.</summary>
    public void Update(AuthorizationPolicyEffect effect, int priority)
    {
        if (!Enum.IsDefined(effect)) throw new ArgumentOutOfRangeException(nameof(effect));
        if (priority is < 0 or > 10000) throw new ArgumentOutOfRangeException(nameof(priority), "정책 우선순위는 0~10000이어야 합니다.");
        Effect = effect;
        Priority = priority;
    }

    /// <summary>활성 정책을 비활성화하거나 다시 활성화합니다.</summary>
    public void SetEnabled(bool enabled) => IsEnabled = enabled;

    private static string Validate(string value, int maximum, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var normalized = value.Trim();
        if (normalized.Length > maximum) throw new ArgumentException($"{name}은 {maximum}자를 초과할 수 없습니다.", name);
        return normalized;
    }
}

/// <summary>권한 평가 결과를 감사 기록과 API 응답에서 공통으로 사용합니다.</summary>
public sealed record AuthorizationDecision(bool IsAllowed, string ReasonCode, string? MatchedPolicyId);

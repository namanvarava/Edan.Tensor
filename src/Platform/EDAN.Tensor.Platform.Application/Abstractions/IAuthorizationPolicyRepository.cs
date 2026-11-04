using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>접근 정책 저장소는 요청 범위에 적용되는 정책을 조회하고 새 정책을 저장합니다.</summary>
public interface IAuthorizationPolicyRepository
{
    Task<IReadOnlyList<AuthorizationPolicy>> GetApplicableAsync(TenantId tenantId, OrganizationId? organizationId,
        ProjectId? projectId, string subjectId, string action, string resourceType, CancellationToken cancellationToken = default);
    void Add(AuthorizationPolicy policy);
}

/// <summary>권한 감사 저장소는 평가 결과를 추가 전용으로 기록합니다.</summary>
public interface IAuthorizationAuditRepository
{
    void Add(AuthorizationAuditEntry entry);
}

using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>감사 보존 정책 저장소는 Tenant별 정책과 변경 이력을 관리합니다.</summary>
public interface IAuthorizationAuditRetentionRepository
{
    Task<AuthorizationAuditRetentionPolicy?> GetByTenantAsync(TenantId tenantId,
        CancellationToken cancellationToken = default);
    void Add(AuthorizationAuditRetentionPolicy policy);
    void AddChange(AuthorizationAuditRetentionChange change);
}

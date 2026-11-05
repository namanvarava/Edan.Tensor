using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>감사 아카이브 작업 저장소는 Tenant별 작업과 실행 이력을 관리합니다.</summary>
public interface IAuthorizationAuditArchiveJobRepository
{
    Task<AuthorizationAuditArchiveJob?> GetAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthorizationAuditArchiveAttempt>> GetAttemptsAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default);
    void Add(AuthorizationAuditArchiveJob job);
    void AddAttempt(AuthorizationAuditArchiveAttempt attempt);
}

using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>테넌트 소속 저장소는 외부 인증 주체의 활성 권한을 확인합니다.</summary>
public interface ITenantMembershipRepository
{
    Task<TenantMembership?> GetAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveMembershipAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default);
    void Add(TenantMembership membership);
}

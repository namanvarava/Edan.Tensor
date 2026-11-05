using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>아카이브 예약 저장소는 Tenant별 정책과 원자적 실행 임대를 관리합니다.</summary>
public interface IAuthorizationAuditArchiveScheduleRepository
{
    Task<AuthorizationAuditArchiveSchedule?> GetAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantId>> GetDueTenantsAsync(DateTimeOffset nowUtc, int take, CancellationToken cancellationToken = default);
    Task<bool> TryAcquireAsync(TenantId tenantId, Guid leaseId, DateTimeOffset nowUtc,
        DateTimeOffset leaseExpiresAtUtc, CancellationToken cancellationToken = default);
    Task CompleteAsync(TenantId tenantId, Guid leaseId, DateTimeOffset nowUtc,
        DateTimeOffset? archivedThroughUtc, bool succeeded,
        CancellationToken cancellationToken = default);
    void Add(AuthorizationAuditArchiveSchedule schedule);
}

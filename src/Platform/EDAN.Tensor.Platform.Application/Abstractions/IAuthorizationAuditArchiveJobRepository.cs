using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>감사 아카이브 작업 저장소는 Tenant별 작업과 실행 이력을 관리합니다.</summary>
public interface IAuthorizationAuditArchiveJobRepository
{
    Task<AuthorizationAuditArchiveJobPage> GetPageAsync(TenantId tenantId, string? status,
        DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("아카이브 작업 페이지 조회가 구현되지 않았습니다.");
    Task<AuthorizationAuditArchiveJob?> GetAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthorizationAuditArchiveAttempt>> GetAttemptsAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default);
    void Add(AuthorizationAuditArchiveJob job);
    void AddAttempt(AuthorizationAuditArchiveAttempt attempt);
}

/// <summary>Tenant 범위 감사 아카이브 작업의 필터·페이징 결과입니다.</summary>
public sealed record AuthorizationAuditArchiveJobPage(
    IReadOnlyList<AuthorizationAuditArchiveJob> Items, int TotalCount, int Page, int PageSize);

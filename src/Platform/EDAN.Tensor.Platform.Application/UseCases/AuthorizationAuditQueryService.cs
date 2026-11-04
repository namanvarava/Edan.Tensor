using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>권한 평가 감사 기록을 Tenant 범위와 제한된 페이지 크기로 조회합니다.</summary>
public sealed class AuthorizationAuditQueryService(IAuthorizationAuditRepository audits)
{
    /// <summary>필터 조건을 검증하고 권한 감사 기록 페이지를 반환합니다.</summary>
    public Task<AuthorizationAuditPage> GetPageAsync(TenantId tenantId, string? subjectId,
        string? action, bool? isAllowed, DateTimeOffset? fromUtc, DateTimeOffset? toUtc,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize), "페이지 크기는 1~100이어야 합니다.");
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
            throw new ArgumentException("조회 시작 시각은 종료 시각보다 늦을 수 없습니다.");
        if (subjectId?.Length > 200) throw new ArgumentException("주체 식별자는 200자를 초과할 수 없습니다.");
        if (action?.Length > 120) throw new ArgumentException("작업 식별자는 120자를 초과할 수 없습니다.");

        return audits.GetPageAsync(tenantId, subjectId, action, isAllowed,
            fromUtc, toUtc, page, pageSize, cancellationToken);
    }
}

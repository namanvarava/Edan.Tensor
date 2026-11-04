using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>테넌트 구성원 부여는 플랫폼 관리자 권한으로 승인된 요청에서 수행합니다.</summary>
public sealed class TenantMembershipService(
    ITenantRepository tenants,
    ITenantMembershipRepository memberships,
    IPlatformUnitOfWork unitOfWork)
{
    /// <summary>외부 인증 주체가 소속된 모든 활성 Tenant를 조회합니다.</summary>
    public Task<IReadOnlyList<TenantMembership>> GetMyMembershipsAsync(string subjectId, CancellationToken cancellationToken = default) =>
        memberships.GetActiveForSubjectAsync(subjectId, cancellationToken);

    /// <summary>활성 테넌트에 외부 인증 주체의 역할을 부여합니다.</summary>
    public async Task<Result<TenantMembershipId>> GrantAsync(
        TenantId tenantId, string subjectId, TenantRole role, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Result<TenantMembershipId>.Failure(new Error("platform.tenant.unavailable", "활성 상태의 테넌트에만 구성원을 추가할 수 있습니다."));

            if (await memberships.ExistsAsync(tenantId, subjectId, cancellationToken))
                return Result<TenantMembershipId>.Failure(new Error("platform.membership.conflict", "해당 사용자는 이미 테넌트 구성원으로 등록되어 있습니다."));

            var membership = TenantMembership.Grant(new TenantMembershipId(Guid.NewGuid()), tenantId, subjectId, role);
            memberships.Add(membership);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<TenantMembershipId>.Success(membership.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<TenantMembershipId>.Failure(new Error("platform.validation", ex.Message));
        }
    }
}

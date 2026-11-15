using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
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

    /// <summary>테넌트 구성원의 역할을 변경하거나 접근 권한을 회수합니다.</summary>
    public async Task<Result> UpdateAsync(
        TenantId tenantId, TenantMembershipId membershipId, TenantRole? role, bool revoke,
        CancellationToken cancellationToken = default)
    {
        var membership = await memberships.GetByIdAsync(tenantId, membershipId, cancellationToken);
        if (membership is null)
            return Result.Failure(new Error("platform.membership.not_found", "테넌트 멤버십을 찾을 수 없습니다."));
        if (!revoke && !role.HasValue)
            return Result.Failure(new Error("platform.validation", "Role 또는 Revoke 중 하나가 필요합니다."));
        try
        {
            if (revoke) membership.Revoke();
            else membership.ChangeRole(role!.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }
}

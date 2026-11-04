using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Results;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>플랫폼 생성 업무는 소유권과 테넌트 격리를 검증한 뒤 저장합니다.</summary>
public sealed class PlatformProvisioningService(
    ITenantRepository tenants,
    IOrganizationRepository organizations,
    IProjectRepository projects,
    IUserRepository users,
    IResourceRepository resources,
    IPolicyRepository policies,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
    public async Task<Result<TenantId>> CreateTenantAsync(string code, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantCode = new TenantCode(code);
            if (await tenants.ExistsCodeAsync(tenantCode, cancellationToken))
                return Fail<TenantId>("platform.tenant.code_conflict", "이미 사용 중인 테넌트 코드입니다.");
            var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), tenantCode, name, clock.UtcNow);
            tenants.Add(tenant);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<TenantId>.Success(tenant.Id);
        }
        catch (DomainException ex) { return Fail<TenantId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<TenantId>("platform.validation", ex.Message); }
    }

    /// <summary>준비 중인 테넌트를 활성화하여 후속 업무를 허용합니다.</summary>
    public async Task<Result> ActivateTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null) return Result.Failure(new Error("platform.tenant.not_found", "테넌트를 찾을 수 없습니다."));
        try
        {
            tenant.Activate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
    }

    public async Task<Result<OrganizationId>> CreateOrganizationAsync(TenantId tenantId, string code, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Fail<OrganizationId>("platform.tenant.unavailable", "활성 상태의 테넌트만 조직을 생성할 수 있습니다.");
            var organizationCode = new OrganizationCode(code);
            if (await organizations.ExistsCodeAsync(tenantId, organizationCode, cancellationToken))
                return Fail<OrganizationId>("platform.organization.code_conflict", "해당 테넌트에서 이미 사용 중인 조직 코드입니다.");
            var organization = Organization.Create(new OrganizationId(Guid.NewGuid()), tenantId, organizationCode, name);
            organizations.Add(organization);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<OrganizationId>.Success(organization.Id);
        }
        catch (DomainException ex) { return Fail<OrganizationId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<OrganizationId>("platform.validation", ex.Message); }
    }

    public async Task<Result<ProjectId>> CreateProjectAsync(TenantId tenantId, OrganizationId organizationId, string code, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Fail<ProjectId>("platform.tenant.unavailable", "활성 상태의 테넌트만 프로젝트를 생성할 수 있습니다.");
            var organization = await organizations.GetByIdAsync(organizationId, cancellationToken);
            if (organization is null || organization.Status != OrganizationStatus.Active || organization.TenantId != tenantId)
                return Fail<ProjectId>("platform.organization.scope_mismatch", "조직이 존재하지 않거나 요청한 테넌트에 속하지 않습니다.");
            var projectCode = new ProjectCode(code);
            if (await projects.ExistsCodeAsync(organizationId, projectCode, cancellationToken))
                return Fail<ProjectId>("platform.project.code_conflict", "해당 조직에서 이미 사용 중인 프로젝트 코드입니다.");
            var project = Project.Create(new ProjectId(Guid.NewGuid()), tenantId, organizationId, projectCode, name);
            projects.Add(project);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<ProjectId>.Success(project.Id);
        }
        catch (DomainException ex) { return Fail<ProjectId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<ProjectId>("platform.validation", ex.Message); }
    }

    public async Task<Result<UserId>> InviteUserAsync(TenantId tenantId, string email, string displayName, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Fail<UserId>("platform.tenant.unavailable", "활성 상태의 테넌트에만 사용자를 초대할 수 있습니다.");
            var address = new EmailAddress(email);
            if (await users.ExistsEmailAsync(tenantId, address, cancellationToken))
                return Fail<UserId>("platform.user.email_conflict", "해당 테넌트에서 이미 사용 중인 이메일 주소입니다.");
            var user = User.Invite(new UserId(Guid.NewGuid()), tenantId, address, displayName);
            users.Add(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<UserId>.Success(user.Id);
        }
        catch (DomainException ex) { return Fail<UserId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<UserId>("platform.validation", ex.Message); }
    }

    public async Task<Result<ResourceId>> CreateResourceAsync(ResourceScope scope, string resourceType, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var scopeError = await ValidateScopeAsync(scope, cancellationToken);
            if (scopeError is not null) return Result<ResourceId>.Failure(scopeError);
            var resource = Resource.Create(new ResourceId(Guid.NewGuid()), scope, resourceType, name);
            resources.Add(resource);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<ResourceId>.Success(resource.Id);
        }
        catch (DomainException ex) { return Fail<ResourceId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<ResourceId>("platform.validation", ex.Message); }
    }

    public async Task<Result<PolicyId>> CreatePolicyAsync(ResourceScope scope, string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            var scopeError = await ValidateScopeAsync(scope, cancellationToken);
            if (scopeError is not null) return Result<PolicyId>.Failure(scopeError);
            var policy = Policy.Create(new PolicyId(Guid.NewGuid()), scope, key, value);
            policies.Add(policy);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<PolicyId>.Success(policy.Id);
        }
        catch (DomainException ex) { return Fail<PolicyId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Fail<PolicyId>("platform.validation", ex.Message); }
    }

    /// <summary>자원과 정책은 테넌트·조직·프로젝트의 실제 소속을 모두 검증합니다.</summary>
    private async Task<Error?> ValidateScopeAsync(ResourceScope scope, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(scope.TenantId, cancellationToken);
        if (tenant is null || tenant.Status != TenantStatus.Active)
            return new Error("platform.tenant.unavailable", "활성 상태의 테넌트 범위만 사용할 수 있습니다.");

        if (scope.OrganizationId is not null)
        {
            var organization = await organizations.GetByIdAsync(scope.OrganizationId, cancellationToken);
            if (organization is null || organization.Status != OrganizationStatus.Active || organization.TenantId != scope.TenantId)
                return new Error("platform.organization.scope_mismatch", "조직이 요청한 테넌트에 속하지 않거나 활성 상태가 아닙니다.");
        }

        if (scope.ProjectId is not null)
        {
            var project = await projects.GetByIdAsync(scope.ProjectId, cancellationToken);
            if (project is null || project.Status != ProjectStatus.Active ||
                project.TenantId != scope.TenantId || project.OrganizationId != scope.OrganizationId)
                return new Error("platform.project.scope_mismatch", "프로젝트의 테넌트·조직 소속이 요청 범위와 일치하지 않습니다.");
        }
        return null;
    }

    private static Result<T> Fail<T>(string code, string description) => Result<T>.Failure(new Error(code, description));
}

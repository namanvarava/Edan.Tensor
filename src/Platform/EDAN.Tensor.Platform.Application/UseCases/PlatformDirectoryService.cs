using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>Registry 화면이 사용하는 Tenant·조직·프로젝트·사용자·멤버십 조회와 lifecycle 명령을 제공합니다.</summary>
public sealed class PlatformDirectoryService(
    ITenantRepository tenants,
    IOrganizationRepository organizations,
    IProjectRepository projects,
    IUserRepository users,
    ITenantMembershipRepository tenantMemberships,
    IScopedMembershipRepository scopedMemberships,
    IPlatformUnitOfWork unitOfWork)
{
    public async Task<PlatformPage<TenantDirectoryView>> GetTenantsAsync(
        string? search, TenantStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var items = await tenants.GetAllAsync(cancellationToken);
        var query = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Code.Value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.Code.Value).Select(x => ToView(x)!), page, pageSize);
    }

    public async Task<TenantDirectoryView?> GetTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default) =>
        ToView(await tenants.GetByIdAsync(tenantId, cancellationToken));

    public async Task<PlatformPage<OrganizationDirectoryView>> GetOrganizationsAsync(
        TenantId tenantId, string? search, OrganizationStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var items = await organizations.GetForTenantAsync(tenantId, cancellationToken);
        var query = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Code.Value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.Code.Value).Select(x => ToView(x)!), page, pageSize);
    }

    public async Task<OrganizationDirectoryView?> GetOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        ToView(await organizations.GetByIdAsync(organizationId, cancellationToken));

    public async Task<PlatformPage<ProjectDirectoryView>> GetProjectsAsync(
        TenantId tenantId, OrganizationId? organizationId, string? search, ProjectStatus? status,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var items = await projects.GetForTenantAsync(tenantId, organizationId, cancellationToken);
        var query = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Code.Value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.Code.Value).Select(x => ToView(x)!), page, pageSize);
    }

    public async Task<ProjectDirectoryView?> GetProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default) =>
        ToView(await projects.GetByIdAsync(projectId, cancellationToken));

    public async Task<PlatformPage<UserDirectoryView>> GetUsersAsync(
        TenantId tenantId, string? search, UserStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var items = await users.GetForTenantAsync(tenantId, cancellationToken);
        var query = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Email.Value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                x.DisplayName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.Email.Value).Select(x => ToView(x)!), page, pageSize);
    }

    public async Task<UserDirectoryView?> GetUserAsync(UserId userId, CancellationToken cancellationToken = default) =>
        ToView(await users.GetByIdAsync(userId, cancellationToken));

    public async Task<IReadOnlyList<TenantMembershipView>> GetTenantMembershipsAsync(TenantId tenantId, CancellationToken cancellationToken = default) =>
        (await tenantMemberships.GetForTenantAsync(tenantId, cancellationToken)).Select(x =>
            new TenantMembershipView(x.Id.Value, x.TenantId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())).ToArray();

    public async Task<PlatformPage<TenantMembershipView>> GetTenantMembershipsPageAsync(
        TenantId tenantId, string? subjectId, TenantMembershipStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var query = (await tenantMemberships.GetForTenantAsync(tenantId, cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(subjectId))
            query = query.Where(x => x.SubjectId.Contains(subjectId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.SubjectId).Select(x =>
            new TenantMembershipView(x.Id.Value, x.TenantId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())), page, pageSize);
    }

    public async Task<IReadOnlyList<OrganizationMembershipView>> GetOrganizationMembershipsAsync(
        TenantId tenantId, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        (await scopedMemberships.GetOrganizationMembershipsAsync(tenantId, organizationId, cancellationToken)).Select(x =>
            new OrganizationMembershipView(x.Id.Value, x.TenantId.Value, x.OrganizationId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())).ToArray();

    public async Task<PlatformPage<OrganizationMembershipView>> GetOrganizationMembershipsPageAsync(
        TenantId tenantId, OrganizationId organizationId, string? subjectId, ScopedMembershipStatus? status,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var query = (await scopedMemberships.GetOrganizationMembershipsAsync(tenantId, organizationId, cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(subjectId))
            query = query.Where(x => x.SubjectId.Contains(subjectId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.SubjectId).Select(x =>
            new OrganizationMembershipView(x.Id.Value, x.TenantId.Value, x.OrganizationId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())), page, pageSize);
    }

    public async Task<IReadOnlyList<ProjectMembershipView>> GetProjectMembershipsAsync(
        TenantId tenantId, OrganizationId organizationId, ProjectId projectId, CancellationToken cancellationToken = default) =>
        (await scopedMemberships.GetProjectMembershipsAsync(tenantId, organizationId, projectId, cancellationToken)).Select(x =>
            new ProjectMembershipView(x.Id.Value, x.TenantId.Value, x.OrganizationId.Value, x.ProjectId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())).ToArray();

    public async Task<PlatformPage<ProjectMembershipView>> GetProjectMembershipsPageAsync(
        TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string? subjectId,
        ScopedMembershipStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        var query = (await scopedMemberships.GetProjectMembershipsAsync(tenantId, organizationId, projectId, cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(subjectId))
            query = query.Where(x => x.SubjectId.Contains(subjectId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return Page(query.OrderBy(x => x.SubjectId).Select(x =>
            new ProjectMembershipView(x.Id.Value, x.TenantId.Value, x.OrganizationId.Value, x.ProjectId.Value, x.SubjectId, x.Role.ToString(), x.Status.ToString())), page, pageSize);
    }

    public async Task<Result> UpdateTenantAsync(TenantId tenantId, string? name, TenantStatus? status, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null) return NotFound("platform.tenant.not_found", "테넌트를 찾을 수 없습니다.");
        try
        {
            if (!string.IsNullOrWhiteSpace(name)) tenant.Rename(name);
            ApplyTenantStatus(tenant, status);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    public async Task<Result> UpdateOrganizationAsync(OrganizationId organizationId, string? name, OrganizationStatus? status, CancellationToken cancellationToken = default)
    {
        var organization = await organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null) return NotFound("platform.organization.not_found", "조직을 찾을 수 없습니다.");
        try
        {
            if (!string.IsNullOrWhiteSpace(name)) organization.Rename(name);
            ApplyOrganizationStatus(organization, status);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    public async Task<Result> UpdateProjectAsync(ProjectId projectId, string? name, ProjectStatus? status, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null) return NotFound("platform.project.not_found", "프로젝트를 찾을 수 없습니다.");
        try
        {
            if (!string.IsNullOrWhiteSpace(name)) project.Rename(name);
            ApplyProjectStatus(project, status);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    public async Task<Result> UpdateUserAsync(UserId userId, string? displayName, UserStatus? status, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return NotFound("platform.user.not_found", "사용자를 찾을 수 없습니다.");
        try
        {
            if (!string.IsNullOrWhiteSpace(displayName)) user.Rename(displayName);
            ApplyUserStatus(user, status);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    private static void ApplyTenantStatus(Tenant tenant, TenantStatus? status)
    {
        if (!status.HasValue || status.Value == tenant.Status) return;
        switch (status.Value)
        {
            case TenantStatus.Active: tenant.Activate(); break;
            case TenantStatus.Suspended: tenant.Suspend(); break;
            case TenantStatus.Closed: tenant.Close(); break;
            default: throw new BusinessRuleViolationException("platform.tenant.invalid_transition", "Provisioning 상태로 되돌릴 수 없습니다.");
        }
    }

    private static void ApplyOrganizationStatus(Organization organization, OrganizationStatus? status)
    {
        if (!status.HasValue || status.Value == organization.Status) return;
        switch (status.Value)
        {
            case OrganizationStatus.Active: organization.Activate(); break;
            case OrganizationStatus.Suspended: organization.Suspend(); break;
            case OrganizationStatus.Closed: organization.Close(); break;
            default: throw new BusinessRuleViolationException("platform.organization.invalid_transition", "지원하지 않는 조직 상태입니다.");
        }
    }

    private static void ApplyProjectStatus(Project project, ProjectStatus? status)
    {
        if (!status.HasValue || status.Value == project.Status) return;
        switch (status.Value)
        {
            case ProjectStatus.Active: project.Activate(); break;
            case ProjectStatus.Suspended: project.Suspend(); break;
            case ProjectStatus.Archived: project.Archive(); break;
            default: throw new BusinessRuleViolationException("platform.project.invalid_transition", "지원하지 않는 프로젝트 상태입니다.");
        }
    }

    private static void ApplyUserStatus(User user, UserStatus? status)
    {
        if (!status.HasValue || status.Value == user.Status) return;
        switch (status.Value)
        {
            case UserStatus.Active: user.Activate(); break;
            case UserStatus.Suspended: user.Suspend(); break;
            case UserStatus.Deactivated: user.Deactivate(); break;
            default: throw new BusinessRuleViolationException("platform.user.invalid_transition", "Invited 상태로 되돌릴 수 없습니다.");
        }
    }

    private static PlatformPage<T> Page<T>(IEnumerable<T> items, int page, int pageSize)
    {
        var all = items.ToArray();
        return new PlatformPage<T>(all.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), all.Length, page, pageSize);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
    }

    private static Result NotFound(string code, string message) => Result.Failure(new Error(code, message));
    private static TenantDirectoryView? ToView(Tenant? x) => x is null ? null : new(x.Id.Value, x.Code.Value, x.Name, x.Status.ToString(), x.CreatedAtUtc);
    private static OrganizationDirectoryView? ToView(Organization? x) => x is null ? null : new(x.Id.Value, x.TenantId.Value, x.Code.Value, x.Name, x.Status.ToString());
    private static ProjectDirectoryView? ToView(Project? x) => x is null ? null : new(x.Id.Value, x.TenantId.Value, x.OrganizationId.Value, x.Code.Value, x.Name, x.Status.ToString());
    private static UserDirectoryView? ToView(User? x) => x is null ? null : new(x.Id.Value, x.TenantId.Value, x.Email.Value, x.DisplayName, x.Status.ToString());
}

public sealed record PlatformPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record TenantDirectoryView(Guid Id, string Code, string Name, string Status, DateTimeOffset CreatedAtUtc);
public sealed record OrganizationDirectoryView(Guid Id, Guid TenantId, string Code, string Name, string Status);
public sealed record ProjectDirectoryView(Guid Id, Guid TenantId, Guid OrganizationId, string Code, string Name, string Status);
public sealed record UserDirectoryView(Guid Id, Guid TenantId, string Email, string DisplayName, string Status);
public sealed record TenantMembershipView(Guid Id, Guid TenantId, string SubjectId, string Role, string Status);
public sealed record OrganizationMembershipView(Guid Id, Guid TenantId, Guid OrganizationId, string SubjectId, string Role, string Status);
public sealed record ProjectMembershipView(Guid Id, Guid TenantId, Guid OrganizationId, Guid ProjectId, string SubjectId, string Role, string Status);

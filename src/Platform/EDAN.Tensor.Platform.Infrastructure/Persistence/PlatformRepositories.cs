using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

/// <summary>EF Core 저장소 구현은 Application 계약을 데이터베이스 조회로 연결합니다.</summary>
public sealed class TenantRepository(PlatformDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default) => db.Tenants.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ExistsCodeAsync(TenantCode code, CancellationToken cancellationToken = default) => db.Tenants.AnyAsync(x => x.Code == code, cancellationToken);
    public void Add(Tenant tenant) => db.Tenants.Add(tenant);
}

public sealed class OrganizationRepository(PlatformDbContext db) : IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken = default) => db.Organizations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ExistsCodeAsync(TenantId tenantId, OrganizationCode code, CancellationToken cancellationToken = default) => db.Organizations.AnyAsync(x => x.TenantId == tenantId && x.Code == code, cancellationToken);
    public void Add(Organization organization) => db.Organizations.Add(organization);
}

public sealed class ProjectRepository(PlatformDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) => db.Projects.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ExistsCodeAsync(OrganizationId organizationId, ProjectCode code, CancellationToken cancellationToken = default) => db.Projects.AnyAsync(x => x.OrganizationId == organizationId && x.Code == code, cancellationToken);
    public void Add(Project project) => db.Projects.Add(project);
}

public sealed class UserRepository(PlatformDbContext db) : IUserRepository
{
    public Task<bool> ExistsEmailAsync(TenantId tenantId, EmailAddress email, CancellationToken cancellationToken = default) => db.Users.AnyAsync(x => x.TenantId == tenantId && x.Email == email, cancellationToken);
    public void Add(User user) => db.Users.Add(user);
}

public sealed class TenantMembershipRepository(PlatformDbContext db) : ITenantMembershipRepository
{
    public Task<TenantMembership?> GetAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default) =>
        db.TenantMemberships.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.SubjectId == subjectId, cancellationToken);
    public Task<bool> ExistsAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default) =>
        db.TenantMemberships.AnyAsync(x => x.TenantId == tenantId && x.SubjectId == subjectId, cancellationToken);
    public Task<bool> HasActiveMembershipAsync(TenantId tenantId, string subjectId, CancellationToken cancellationToken = default) =>
        db.TenantMemberships.AnyAsync(x => x.TenantId == tenantId && x.SubjectId == subjectId && x.Status == TenantMembershipStatus.Active, cancellationToken);
    public async Task<IReadOnlyList<TenantMembership>> GetActiveForSubjectAsync(string subjectId, CancellationToken cancellationToken = default) =>
        await db.TenantMemberships.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.Status == TenantMembershipStatus.Active)
            .OrderBy(x => x.TenantId)
            .ToListAsync(cancellationToken);

    public void Add(TenantMembership membership) => db.TenantMemberships.Add(membership);
}

public sealed class ResourceRepository(PlatformDbContext db) : IResourceRepository
{
    public void Add(Resource resource) => db.Resources.Add(resource);
}

public sealed class PolicyRepository(PlatformDbContext db) : IPolicyRepository
{
    public void Add(Policy policy) => db.Policies.Add(policy);
}

/// <summary>동일 DbContext의 변경 사항을 하나의 저장 작업으로 확정합니다.</summary>
public sealed class PlatformUnitOfWork(PlatformDbContext db) : IPlatformUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}

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

/// <summary>조직·프로젝트 소속의 중복 여부와 쓰기 권한을 조회합니다.</summary>
public sealed class ScopedMembershipRepository(PlatformDbContext db) : IScopedMembershipRepository
{
    public Task<bool> OrganizationMembershipExistsAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) =>
        db.OrganizationMemberships.AnyAsync(x => x.TenantId == tenantId && x.OrganizationId == organizationId && x.SubjectId == subjectId, cancellationToken);

    public Task<bool> ProjectMembershipExistsAsync(TenantId tenantId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default) =>
        db.ProjectMemberships.AnyAsync(x => x.TenantId == tenantId && x.ProjectId == projectId && x.SubjectId == subjectId, cancellationToken);

    public Task<bool> HasOrganizationWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) =>
        db.OrganizationMemberships.AnyAsync(x => x.TenantId == tenantId && x.OrganizationId == organizationId &&
            x.SubjectId == subjectId && x.Status == ScopedMembershipStatus.Active &&
            (x.Role == OrganizationRole.Contributor || x.Role == OrganizationRole.Manager), cancellationToken);

    public Task<bool> HasOrganizationManagementAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) =>
        db.OrganizationMemberships.AnyAsync(x => x.TenantId == tenantId && x.OrganizationId == organizationId &&
            x.SubjectId == subjectId && x.Status == ScopedMembershipStatus.Active && x.Role == OrganizationRole.Manager, cancellationToken);

    public Task<bool> HasProjectWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default) =>
        db.ProjectMemberships.AnyAsync(x => x.TenantId == tenantId && x.OrganizationId == organizationId && x.ProjectId == projectId &&
            x.SubjectId == subjectId && x.Status == ScopedMembershipStatus.Active &&
            (x.Role == ProjectRole.Contributor || x.Role == ProjectRole.Manager), cancellationToken);

    public void Add(OrganizationMembership membership) => db.OrganizationMemberships.Add(membership);
    public void Add(ProjectMembership membership) => db.ProjectMemberships.Add(membership);
}

/// <summary>접근 정책을 계층형 범위에서 조회하고 감사 기록을 추가합니다.</summary>
public sealed class AuthorizationPolicyRepository(PlatformDbContext db) : IAuthorizationPolicyRepository
{
    public async Task<IReadOnlyList<AuthorizationPolicy>> GetApplicableAsync(TenantId tenantId,
        OrganizationId? organizationId, ProjectId? projectId, string subjectId, string action,
        string resourceType, CancellationToken cancellationToken = default) =>
        await db.AuthorizationPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SubjectId == subjectId && x.IsEnabled &&
                x.Action == action.ToLower() && (x.ResourceType == resourceType || x.ResourceType == "*") &&
                (x.OrganizationId == null || x.OrganizationId == organizationId) &&
                (x.ProjectId == null || x.ProjectId == projectId))
            .OrderByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuthorizationPolicy>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        // SQLite는 DateTimeOffset 정렬을 SQL로 변환하지 못하므로 조회 후 UTC 시각 기준으로 정렬합니다.
        var policies = await db.AuthorizationPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        return policies.OrderByDescending(x => x.CreatedAtUtc).ToArray();
    }

    public Task<AuthorizationPolicy?> GetByIdAsync(AuthorizationPolicyId id, CancellationToken cancellationToken = default) =>
        db.AuthorizationPolicies.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(AuthorizationPolicy policy) => db.AuthorizationPolicies.Add(policy);

    public void AddChangeAudit(AuthorizationPolicyChangeAuditEntry entry) =>
        db.AuthorizationPolicyChangeAuditEntries.Add(entry);
}


/// <summary>감사 보존 정책과 변경 이력을 Tenant 범위로 저장합니다.</summary>
public sealed class AuthorizationAuditRetentionRepository(PlatformDbContext db) : IAuthorizationAuditRetentionRepository
{
    public Task<AuthorizationAuditRetentionPolicy?> GetByTenantAsync(TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        db.AuthorizationAuditRetentionPolicies.SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public void Add(AuthorizationAuditRetentionPolicy policy) => db.AuthorizationAuditRetentionPolicies.Add(policy);

    public void AddChange(AuthorizationAuditRetentionChange change) =>
        db.AuthorizationAuditRetentionChanges.Add(change);
}

/// <summary>감사 기록 저장소는 평가 결과를 삭제하거나 수정하지 않고 추가합니다.</summary>
public sealed class AuthorizationAuditRepository(PlatformDbContext db) : IAuthorizationAuditRepository
{
    /// <summary>Tenant 경계를 유지하며 필터와 페이지 조건에 맞는 권한 평가 기록을 조회합니다.</summary>
    public async Task<AuthorizationAuditPage> GetPageAsync(TenantId tenantId, string? subjectId,
        string? action, bool? isAllowed, DateTimeOffset? fromUtc, DateTimeOffset? toUtc,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = db.AuthorizationAuditEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(subjectId))
            query = query.Where(x => x.SubjectId == subjectId);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);
        if (isAllowed.HasValue)
            query = query.Where(x => x.IsAllowed == isAllowed.Value);
        if (fromUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value.ToUniversalTime());
        if (toUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value.ToUniversalTime());

        var totalCount = await query.CountAsync(cancellationToken);
        // SQLite 공급자는 DateTimeOffset 정렬을 SQL로 변환하지 못하므로 필터 결과를 UTC 시각으로 정렬합니다.
        var ordered = (await query.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return new AuthorizationAuditPage(ordered, totalCount, page, pageSize);
    }

    /// <summary>SQLite의 DateTimeOffset 비교 변환 제약을 피하고 Tenant 범위에서 UTC 기간을 적용합니다.</summary>
    public async Task<IReadOnlyList<AuthorizationAuditEntry>> GetForIntegrityAsync(TenantId tenantId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        var tenantEntries = await db.AuthorizationAuditEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        var from = fromUtc.ToUniversalTime();
        var to = toUtc.ToUniversalTime();
        return tenantEntries
            .Where(x => x.OccurredAtUtc >= from && x.OccurredAtUtc <= to)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.Id.Value)
            .ToArray();
    }

    public void Add(AuthorizationAuditEntry entry) => db.AuthorizationAuditEntries.Add(entry);
}


/// <summary>감사 아카이브 작업과 실행 이력을 Tenant 범위로 조회하고 추가합니다.</summary>
public sealed class AuthorizationAuditArchiveJobRepository(PlatformDbContext db) : IAuthorizationAuditArchiveJobRepository
{
    public Task<AuthorizationAuditArchiveJob?> GetAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default) =>
        db.AuthorizationAuditArchiveJobs.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == jobId, cancellationToken);

    public async Task<IReadOnlyList<AuthorizationAuditArchiveAttempt>> GetAttemptsAsync(TenantId tenantId,
        Guid jobId, CancellationToken cancellationToken = default)
    {
        var attempts = await db.AuthorizationAuditArchiveAttempts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.JobId == jobId)
            .ToListAsync(cancellationToken);
        return attempts.OrderBy(x => x.StartedAtUtc).ThenBy(x => x.Id).ToArray();
    }

    public void Add(AuthorizationAuditArchiveJob job) => db.AuthorizationAuditArchiveJobs.Add(job);
    public void AddAttempt(AuthorizationAuditArchiveAttempt attempt) => db.AuthorizationAuditArchiveAttempts.Add(attempt);
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


/// <summary>예약 아카이브 정책과 분산 실행 임대를 원자적으로 관리합니다.</summary>
public sealed class AuthorizationAuditArchiveScheduleRepository(PlatformDbContext db)
    : IAuthorizationAuditArchiveScheduleRepository
{
    public Task<AuthorizationAuditArchiveSchedule?> GetAsync(TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        db.AuthorizationAuditArchiveSchedules.SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<TenantId>> GetDueTenantsAsync(DateTimeOffset nowUtc, int take,
        CancellationToken cancellationToken = default) =>
        (await db.AuthorizationAuditArchiveSchedules.AsNoTracking()
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken))
            .Where(x => x.NextRunAtUtc <= nowUtc &&
                (x.LeaseExpiresAtUtc == null || x.LeaseExpiresAtUtc <= nowUtc))
            .OrderBy(x => x.NextRunAtUtc)
            .Take(take)
            .Select(x => x.TenantId)
            .ToArray();

    public async Task<bool> TryAcquireAsync(TenantId tenantId, Guid leaseId, DateTimeOffset nowUtc,
        DateTimeOffset leaseExpiresAtUtc, CancellationToken cancellationToken = default)
    {
        var schedule = await db.AuthorizationAuditArchiveSchedules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (schedule is null || !schedule.IsEnabled || schedule.NextRunAtUtc > nowUtc
            || (schedule.LeaseExpiresAtUtc.HasValue && schedule.LeaseExpiresAtUtc.Value > nowUtc))
            return false;

        var affected = await db.AuthorizationAuditArchiveSchedules
            .Where(x => x.TenantId == tenantId && x.Version == schedule.Version && x.IsEnabled)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LeaseId, leaseId)
                .SetProperty(x => x.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                .SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);
        return affected == 1;
    }

    /// <summary>현재 임대 소유자만 만료 시각을 원자적으로 연장합니다.</summary>
    public async Task<bool> RenewLeaseAsync(TenantId tenantId, Guid leaseId, DateTimeOffset nowUtc,
        DateTimeOffset leaseExpiresAtUtc, CancellationToken cancellationToken = default)
    {
        var affected = await db.AuthorizationAuditArchiveSchedules
            .Where(x => x.TenantId == tenantId && x.LeaseId == leaseId &&
                x.LeaseExpiresAtUtc.HasValue && x.LeaseExpiresAtUtc.Value > nowUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                .SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);
        return affected == 1;
    }

    public async Task CompleteAsync(TenantId tenantId, Guid leaseId, DateTimeOffset nowUtc,
        DateTimeOffset? archivedThroughUtc, bool succeeded, CancellationToken cancellationToken = default)
    {
        var schedule = await db.AuthorizationAuditArchiveSchedules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.LeaseId == leaseId, cancellationToken);
        if (schedule is null)
            throw new InvalidOperationException("아카이브 예약 실행 임대가 만료되었거나 다른 작업자가 소유합니다.");

        var nextRunAtUtc = succeeded
            ? (archivedThroughUtc.HasValue && archivedThroughUtc.Value < nowUtc
                ? nowUtc : nowUtc.AddHours(schedule.IntervalHours))
            : nowUtc.AddMinutes(5);
        var affected = await db.AuthorizationAuditArchiveSchedules
            .Where(x => x.TenantId == tenantId && x.LeaseId == leaseId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LastRunAtUtc, succeeded ? archivedThroughUtc : schedule.LastRunAtUtc)
                .SetProperty(x => x.NextRunAtUtc, nextRunAtUtc)
                .SetProperty(x => x.LeaseId, (Guid?)null)
                .SetProperty(x => x.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);
        if (affected != 1)
            throw new InvalidOperationException("아카이브 예약 실행 임대가 만료되었거나 다른 작업자가 소유합니다.");
    }

    public void Add(AuthorizationAuditArchiveSchedule schedule) =>
        db.AuthorizationAuditArchiveSchedules.Add(schedule);
}

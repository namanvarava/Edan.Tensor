using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

/// <summary>Tenant·Organization·Project 범위 권한 평가 규칙을 검증합니다.</summary>
public sealed class PlatformAuthorizationServiceTests
{
    [Fact]
    public async Task TenantAdministratorCanCreateAcrossSelectedTenantScopes()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var subjectId = "tenant-admin";
        var service = CreateService(tenantId, subjectId, TenantRole.Administrator);
        Assert.True(await service.CanCreateInScopeAsync(tenantId, null, null, subjectId));
        Assert.True(await service.CanCreateInScopeAsync(tenantId, new OrganizationId(Guid.NewGuid()), null, subjectId));
        Assert.True(await service.CanCreateInScopeAsync(tenantId, new OrganizationId(Guid.NewGuid()), new ProjectId(Guid.NewGuid()), subjectId));
    }

    [Fact]
    public async Task TenantMemberWithoutScopedMembershipCannotCreateScopedResources()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var service = CreateService(tenantId, "tenant-member", TenantRole.Member);
        Assert.False(await service.CanCreateInScopeAsync(tenantId, null, null, "tenant-member"));
        Assert.False(await service.CanCreateInScopeAsync(tenantId, new OrganizationId(Guid.NewGuid()), null, "tenant-member"));
    }

    [Fact]
    public async Task OrganizationContributorCannotInheritProjectWriteAccess()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var organizationId = new OrganizationId(Guid.NewGuid());
        var service = CreateService(tenantId, "org-contributor", TenantRole.Member,
            organizationWrite: true, organizationManager: false);
        Assert.True(await service.CanCreateInScopeAsync(tenantId, organizationId, null, "org-contributor"));
        Assert.False(await service.CanCreateInScopeAsync(tenantId, organizationId, new ProjectId(Guid.NewGuid()), "org-contributor"));
    }

    [Fact]
    public async Task OrganizationManagerCanInheritProjectWriteAccess()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var service = CreateService(tenantId, "org-manager", TenantRole.Member,
            organizationWrite: true, organizationManager: true);
        Assert.True(await service.CanCreateInScopeAsync(tenantId, new OrganizationId(Guid.NewGuid()),
            new ProjectId(Guid.NewGuid()), "org-manager"));
    }

    [Fact]
    public async Task ProjectContributorCanCreateOnlyWithinAssignedProject()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var organizationId = new OrganizationId(Guid.NewGuid());
        var projectId = new ProjectId(Guid.NewGuid());
        var service = CreateService(tenantId, "project-contributor", TenantRole.Member, projectWrite: true, assignedProjectId: projectId);
        Assert.True(await service.CanCreateInScopeAsync(tenantId, organizationId, projectId, "project-contributor"));
        Assert.False(await service.CanCreateInScopeAsync(tenantId, organizationId, new ProjectId(Guid.NewGuid()), "project-contributor"));
    }

    [Fact]
    public async Task ExplicitDenyPolicyOverridesTenantAdministratorRole()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var subjectId = "tenant-admin";
        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenantId,
            null, null, subjectId, "resource:create", "*", AuthorizationPolicyEffect.Deny, 1, DateTimeOffset.UtcNow);
        var policyRepository = new FakePolicyRepository([policy]);
        var auditRepository = new FakeAuditRepository();
        var service = new PlatformAuthorizationService(
            new FakeTenantMembershipRepository(tenantId, subjectId, TenantRole.Administrator),
            new FakeScopedMembershipRepository(false, false, false),
            policyRepository, auditRepository, new FakeUnitOfWork(), new SystemClock());

        var decision = await service.EvaluateAsync(tenantId, null, null, subjectId, "resource:create", "DataSet");

        Assert.False(decision.IsAllowed);
        Assert.Equal("policy.explicit_deny", decision.ReasonCode);
        Assert.Single(auditRepository.Entries);
        Assert.False(auditRepository.Entries[0].IsAllowed);
    }

    [Fact]
    public async Task ExplicitAllowPolicyCanGrantAccessToTenantMember()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var subjectId = "policy-user";
        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenantId,
            null, null, subjectId, "resource:create", "DataSet", AuthorizationPolicyEffect.Allow, 100, DateTimeOffset.UtcNow);
        var service = new PlatformAuthorizationService(
            new FakeTenantMembershipRepository(tenantId, subjectId, TenantRole.Member),
            new FakeScopedMembershipRepository(false, false, false),
            new FakePolicyRepository([policy]), new FakeAuditRepository(), new FakeUnitOfWork(), new SystemClock());

        var decision = await service.EvaluateAsync(tenantId, null, null, subjectId, "resource:create", "DataSet");

        Assert.True(decision.IsAllowed);
        Assert.Equal("policy.explicit_allow", decision.ReasonCode);
    }

    [Fact]
    public async Task ExplicitAllowCannotBypassMissingTenantMembership()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenantId,
            null, null, "unaffiliated-user", "resource:create", "*", AuthorizationPolicyEffect.Allow, 100, DateTimeOffset.UtcNow);
        var service = new PlatformAuthorizationService(
            new FakeTenantMembershipRepository(tenantId, "different-user", TenantRole.Member),
            new FakeScopedMembershipRepository(false, false, false),
            new FakePolicyRepository([policy]), new FakeAuditRepository(), new FakeUnitOfWork(), new SystemClock());

        var decision = await service.EvaluateAsync(tenantId, null, null, "unaffiliated-user", "resource:create", "DataSet");

        Assert.False(decision.IsAllowed);
        Assert.Equal("membership.required", decision.ReasonCode);
    }

    private static PlatformAuthorizationService CreateService(
        TenantId tenantId,
        string subjectId,
        TenantRole tenantRole,
        bool organizationWrite = false,
        bool organizationManager = false,
        bool projectWrite = false,
        ProjectId? assignedProjectId = null)
    {
        var tenantMemberships = new FakeTenantMembershipRepository(tenantId, subjectId, tenantRole);
        var scopedMemberships = new FakeScopedMembershipRepository(organizationWrite, organizationManager, projectWrite, assignedProjectId);
        return new PlatformAuthorizationService(tenantMemberships, scopedMemberships, new FakePolicyRepository(), new FakeAuditRepository(), new FakeUnitOfWork(), new SystemClock());
    }

    private sealed class FakeUnitOfWork : IPlatformUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakePolicyRepository(IReadOnlyList<AuthorizationPolicy>? items = null) : IAuthorizationPolicyRepository
    {
        private readonly IReadOnlyList<AuthorizationPolicy> _items = items ?? [];
        public Task<IReadOnlyList<AuthorizationPolicy>> GetApplicableAsync(TenantId tenantId, OrganizationId? organizationId, ProjectId? projectId, string subjectId, string action, string resourceType, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AuthorizationPolicy>>(_items.Where(x => x.TenantId == tenantId && x.SubjectId == subjectId && x.Action == action && (x.ResourceType == resourceType || x.ResourceType == "*") && (x.OrganizationId is null || x.OrganizationId == organizationId) && (x.ProjectId is null || x.ProjectId == projectId)).ToList());
        public void Add(AuthorizationPolicy policy) => throw new NotSupportedException();
    }

    private sealed class FakeAuditRepository : IAuthorizationAuditRepository
    {
        public List<AuthorizationAuditEntry> Entries { get; } = [];
        public void Add(AuthorizationAuditEntry entry) => Entries.Add(entry);
    }

    private sealed class FakeTenantMembershipRepository(TenantId tenantId, string subjectId, TenantRole role)
        : ITenantMembershipRepository
    {
        private readonly TenantMembership _membership = TenantMembership.Grant(
            new TenantMembershipId(Guid.NewGuid()), tenantId, subjectId, role);
        public Task<TenantMembership?> GetAsync(TenantId id, string subject, CancellationToken cancellationToken = default) =>
            Task.FromResult<TenantMembership?>(id == tenantId && subject == subjectId ? _membership : null);
        public Task<bool> ExistsAsync(TenantId id, string subject, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == tenantId && subject == subjectId);
        public Task<bool> HasActiveMembershipAsync(TenantId id, string subject, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == tenantId && subject == subjectId);
        public Task<IReadOnlyList<TenantMembership>> GetActiveForSubjectAsync(string subject, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TenantMembership>>(subject == subjectId ? [_membership] : []);
        public void Add(TenantMembership membership) => throw new NotSupportedException();
    }

    private sealed class FakeScopedMembershipRepository(bool organizationWrite, bool organizationManager, bool projectWrite, ProjectId? assignedProjectId = null)
        : IScopedMembershipRepository
    {
        public Task<bool> OrganizationMembershipExistsAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ProjectMembershipExistsAsync(TenantId tenantId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> HasOrganizationWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) => Task.FromResult(organizationWrite);
        public Task<bool> HasOrganizationManagementAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default) => Task.FromResult(organizationManager);
        public Task<bool> HasProjectWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default) => Task.FromResult(projectWrite && projectId == assignedProjectId);
        public void Add(OrganizationMembership membership) => throw new NotSupportedException();
        public void Add(ProjectMembership membership) => throw new NotSupportedException();
    }
}

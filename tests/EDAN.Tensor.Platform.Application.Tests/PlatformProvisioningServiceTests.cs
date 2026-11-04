using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

public sealed class PlatformProvisioningServiceTests
{
    [Fact]
    public async Task TenantCreationPersistsProvisioningTenant()
    {
        var store = new Store();
        var result = await Service(store).CreateTenantAsync("acme", "에이씨미");
        Assert.True(result.IsSuccess);
        Assert.Single(store.Tenants);
        Assert.Equal(TenantStatus.Provisioning, store.Tenants[0].Status);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task TenantActivationEnablesOrganizationProvisioning()
    {
        var store = new Store();
        var tenant = AddTenant(store, TenantStatus.Provisioning);
        var service = Service(store);
        var activation = await service.ActivateTenantAsync(tenant.Id);
        var organization = await service.CreateOrganizationAsync(tenant.Id, "OPS", "운영");
        Assert.True(activation.IsSuccess);
        Assert.True(organization.IsSuccess);
        Assert.Equal(TenantStatus.Active, tenant.Status);
    }

    [Fact]
    public async Task OrganizationCreationRejectsSuspendedTenant()
    {
        var store = new Store();
        var tenant = AddTenant(store, TenantStatus.Suspended);
        var result = await Service(store).CreateOrganizationAsync(tenant.Id, "OPS", "운영");
        Assert.True(result.IsFailure);
        Assert.Equal("platform.tenant.unavailable", result.Error.Code);
        Assert.Empty(store.Organizations);
    }

    [Fact]
    public async Task ProjectCreationRejectsOrganizationFromAnotherTenant()
    {
        var store = new Store();
        var tenant = AddTenant(store, TenantStatus.Active);
        var otherTenant = AddTenant(store, TenantStatus.Active);
        var organization = Organization.Create(new(Guid.NewGuid()), otherTenant.Id, new("OPS"), "운영");
        store.Organizations.Add(organization);
        var result = await Service(store).CreateProjectAsync(tenant.Id, organization.Id, "CORE", "핵심");
        Assert.True(result.IsFailure);
        Assert.Equal("platform.organization.scope_mismatch", result.Error.Code);
        Assert.Empty(store.Projects);
    }

    [Fact]
    public async Task UserEmailUniquenessIsTenantScoped()
    {
        var store = new Store();
        var first = AddTenant(store, TenantStatus.Active);
        var second = AddTenant(store, TenantStatus.Active);
        store.Emails.Add((first.Id, "person@example.com"));
        var service = Service(store);
        var rejected = await service.InviteUserAsync(first.Id, "person@example.com", "사용자");
        var accepted = await service.InviteUserAsync(second.Id, "person@example.com", "사용자");
        Assert.True(rejected.IsFailure);
        Assert.True(accepted.IsSuccess);
    }

    [Fact]
    public async Task ResourceCreationRejectsProjectFromDifferentOrganization()
    {
        var store = new Store();
        var tenant = AddTenant(store, TenantStatus.Active);
        var organizationA = Organization.Create(new(Guid.NewGuid()), tenant.Id, new("OPS"), "운영");
        var organizationB = Organization.Create(new(Guid.NewGuid()), tenant.Id, new("DEV"), "개발");
        store.Organizations.AddRange([organizationA, organizationB]);
        var project = Project.Create(new(Guid.NewGuid()), tenant.Id, organizationA.Id, new("CORE"), "핵심");
        store.Projects.Add(project);
        var scope = new ResourceScope(tenant.Id, organizationB.Id, project.Id);
        var result = await Service(store).CreateResourceAsync(scope, "DataSet", "기본 데이터");
        Assert.True(result.IsFailure);
        Assert.Equal("platform.project.scope_mismatch", result.Error.Code);
        Assert.Empty(store.Resources);
    }

    [Fact]
    public async Task PolicyCreationRequiresActiveTenant()
    {
        var store = new Store();
        var tenant = AddTenant(store, TenantStatus.Closed);
        var result = await Service(store).CreatePolicyAsync(new ResourceScope(tenant.Id), "retention.days", "30");
        Assert.True(result.IsFailure);
        Assert.Empty(store.Policies);
    }

    private static Tenant AddTenant(Store store, TenantStatus status)
    {
        var tenant = Tenant.Create(new(Guid.NewGuid()), new("TENANT-" + Guid.NewGuid().ToString("N")[..8]), "테스트", DateTimeOffset.UtcNow);
        if (status == TenantStatus.Active) tenant.Activate();
        else if (status == TenantStatus.Suspended) { tenant.Activate(); tenant.Suspend(); }
        else if (status == TenantStatus.Closed) tenant.Close();
        store.Tenants.Add(tenant);
        return tenant;
    }

    private static PlatformProvisioningService Service(Store store) =>
        new(store, store, store, store, store, store, store, new FixedClock());

    private sealed class FixedClock : ISystemClock
    {
        public DateTimeOffset UtcNow => new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class Store : ITenantRepository, IOrganizationRepository, IProjectRepository,
        IUserRepository, IResourceRepository, IPolicyRepository, IPlatformUnitOfWork
    {
        public List<Tenant> Tenants { get; } = [];
        public List<Organization> Organizations { get; } = [];
        public List<Project> Projects { get; } = [];
        public List<User> Users { get; } = [];
        public List<Resource> Resources { get; } = [];
        public List<Policy> Policies { get; } = [];
        public List<(TenantId TenantId, string Email)> Emails { get; } = [];
        public int SaveCount { get; private set; }

        public Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default) => Task.FromResult(Tenants.SingleOrDefault(x => x.Id == id));
        public Task<bool> ExistsCodeAsync(TenantCode code, CancellationToken cancellationToken = default) => Task.FromResult(Tenants.Any(x => x.Code == code));
        public void Add(Tenant tenant) => Tenants.Add(tenant);
        public Task<Organization?> GetByIdAsync(OrganizationId id, CancellationToken cancellationToken = default) => Task.FromResult(Organizations.SingleOrDefault(x => x.Id == id));
        public Task<bool> ExistsCodeAsync(TenantId tenantId, OrganizationCode code, CancellationToken cancellationToken = default) => Task.FromResult(Organizations.Any(x => x.TenantId == tenantId && x.Code == code));
        public void Add(Organization organization) => Organizations.Add(organization);
        public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) => Task.FromResult(Projects.SingleOrDefault(x => x.Id == id));
        public Task<bool> ExistsCodeAsync(OrganizationId organizationId, ProjectCode code, CancellationToken cancellationToken = default) => Task.FromResult(Projects.Any(x => x.OrganizationId == organizationId && x.Code == code));
        public void Add(Project project) => Projects.Add(project);
        public Task<bool> ExistsEmailAsync(TenantId tenantId, EmailAddress email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Emails.Any(x => x.TenantId == tenantId && x.Email == email.Value) || Users.Any(x => x.TenantId == tenantId && x.Email == email));
        public void Add(User user) => Users.Add(user);
        public void Add(Resource resource) => Resources.Add(resource);
        public void Add(Policy policy) => Policies.Add(policy);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.FromResult(1); }
    }
}

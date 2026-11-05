using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EDAN.Tensor.Platform.Infrastructure.Tests;

public sealed class PlatformInfrastructureTests
{
    [Fact]
    public void ModelDefinesTenantScopedUniqueIndexes()
    {
        using var db = CreateContext();
        var organization = db.Model.FindEntityType(typeof(Organization))!;
        var project = db.Model.FindEntityType(typeof(Project))!;
        var user = db.Model.FindEntityType(typeof(User))!;

        Assert.Contains(organization.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["TenantId", "Code"]));
        Assert.Contains(project.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["OrganizationId", "Code"]));
        Assert.Contains(user.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["TenantId", "Email"]));
    }

    [Fact]
    public async Task TenantRepositoryChecksGlobalTenantCode()
    {
        using var db = CreateContext();
        var repository = new TenantRepository(db);
        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-A"), "테넌트 A", DateTimeOffset.UtcNow);
        repository.Add(tenant);
        await db.SaveChangesAsync();

        Assert.True(await repository.ExistsCodeAsync(new TenantCode("TENANT-A")));
        Assert.False(await repository.ExistsCodeAsync(new TenantCode("TENANT-B")));
    }

    [Fact]
    public async Task UserRepositoryChecksEmailWithinTenantBoundary()
    {
        using var db = CreateContext();
        var repository = new UserRepository(db);
        var firstTenant = new TenantId(Guid.NewGuid());
        var secondTenant = new TenantId(Guid.NewGuid());
        var email = new EmailAddress("person@example.com");

        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), firstTenant, email, "사용자"));
        await db.SaveChangesAsync();

        Assert.True(await repository.ExistsEmailAsync(firstTenant, email));
        Assert.False(await repository.ExistsEmailAsync(secondTenant, email));
    }

    [Fact]
    public async Task SqlitePersistsTenantAndEnforcesUniqueTenantCode()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var repository = new TenantRepository(db);
        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("SQLITE-A"), "SQLite 테넌트", DateTimeOffset.UtcNow);
        repository.Add(tenant);
        await db.SaveChangesAsync();

        Assert.True(await repository.ExistsCodeAsync(new TenantCode("SQLITE-A")));
        Assert.NotNull(await repository.GetByIdAsync(tenant.Id));

        repository.Add(Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("SQLITE-A"), "중복 테넌트", DateTimeOffset.UtcNow));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SqliteAllowsSameEmailAcrossTenantsButRejectsDuplicateWithinTenant()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var firstTenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-A"), "테넌트 A", DateTimeOffset.UtcNow);
        var secondTenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-B"), "테넌트 B", DateTimeOffset.UtcNow);
        db.Tenants.AddRange(firstTenant, secondTenant);
        await db.SaveChangesAsync();

        var email = new EmailAddress("shared@example.com");
        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), firstTenant.Id, email, "첫 번째 사용자"));
        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), secondTenant.Id, email, "두 번째 사용자"));
        await db.SaveChangesAsync();

        Assert.True(await new UserRepository(db).ExistsEmailAsync(firstTenant.Id, email));
        Assert.True(await new UserRepository(db).ExistsEmailAsync(secondTenant.Id, email));

        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), firstTenant.Id, email, "중복 사용자"));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SqliteRejectsProjectAssignedToAnotherTenantsOrganization()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenantA = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-A"), "테넌트 A", DateTimeOffset.UtcNow);
        var tenantB = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-B"), "테넌트 B", DateTimeOffset.UtcNow);
        db.Tenants.AddRange(tenantA, tenantB);
        var organization = Organization.Create(new OrganizationId(Guid.NewGuid()), tenantA.Id,
            new OrganizationCode("ORG-A"), "조직 A");
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var invalidProject = Project.Create(new ProjectId(Guid.NewGuid()), tenantB.Id, organization.Id,
            new ProjectCode("PROJECT-X"), "잘못된 소속 프로젝트");
        db.Projects.Add(invalidProject);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SqliteRejectsResourceProjectFromDifferentOrganization()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("TENANT-A"), "테넌트 A", DateTimeOffset.UtcNow);
        db.Tenants.Add(tenant);
        var organizationA = Organization.Create(new OrganizationId(Guid.NewGuid()), tenant.Id,
            new OrganizationCode("ORG-A"), "조직 A");
        var organizationB = Organization.Create(new OrganizationId(Guid.NewGuid()), tenant.Id,
            new OrganizationCode("ORG-B"), "조직 B");
        db.Organizations.AddRange(organizationA, organizationB);
        var project = Project.Create(new ProjectId(Guid.NewGuid()), tenant.Id, organizationA.Id,
            new ProjectCode("PROJECT-A"), "프로젝트 A");
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var invalidResource = Resource.Create(new ResourceId(Guid.NewGuid()),
            new ResourceScope(tenant.Id, organizationB.Id, project.Id), "Document", "잘못된 범위");
        db.Resources.Add(invalidResource);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SqliteArchiveScheduleLeaseAllowsOnlyOneConcurrentWorker()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()),
            new TenantCode("SCHEDULE-A"), "예약 테스트 Tenant", DateTimeOffset.UtcNow);
        db.Tenants.Add(tenant);
        var now = DateTimeOffset.Parse("2026-11-05T00:00:00Z");
        var schedule = AuthorizationAuditArchiveSchedule.Create(tenant.Id, 1, now.AddHours(-2));
        db.AuthorizationAuditArchiveSchedules.Add(schedule);
        await db.SaveChangesAsync();

        var repository = new AuthorizationAuditArchiveScheduleRepository(db);
        var due = await repository.GetDueTenantsAsync(now, 10);
        Assert.Contains(tenant.Id, due);

        var firstLease = Guid.NewGuid();
        var secondLease = Guid.NewGuid();
        Assert.True(await repository.TryAcquireAsync(tenant.Id, firstLease, now, now.AddMinutes(30)));
        Assert.False(await repository.TryAcquireAsync(tenant.Id, secondLease, now, now.AddMinutes(30)));

        var renewedUntil = now.AddMinutes(40);
        Assert.True(await repository.RenewLeaseAsync(tenant.Id, firstLease,
            now.AddMinutes(10), renewedUntil));
        db.ChangeTracker.Clear();

        // 갱신된 임대가 유효한 동안 다른 작업자는 인수할 수 없습니다.
        Assert.False(await repository.TryAcquireAsync(tenant.Id, secondLease,
            now.AddMinutes(31), now.AddMinutes(61)));
        Assert.False(await repository.RenewLeaseAsync(tenant.Id, secondLease,
            now.AddMinutes(31), now.AddMinutes(61)));

        Assert.False(await repository.RenewLeaseAsync(tenant.Id, firstLease,
            now.AddMinutes(41), now.AddMinutes(71)));
        Assert.True(await repository.TryAcquireAsync(tenant.Id, secondLease,
            now.AddMinutes(41), now.AddMinutes(71)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CompleteAsync(
            tenant.Id, firstLease, now.AddMinutes(42), now.AddMinutes(1), succeeded: true));
        await repository.CompleteAsync(tenant.Id, secondLease,
            now.AddMinutes(42), now.AddMinutes(1), succeeded: true);
        db.ChangeTracker.Clear();
        var updated = await repository.GetAsync(tenant.Id);
        Assert.NotNull(updated);
        Assert.Null(updated.LeaseId);
        Assert.Equal(now.AddMinutes(1), updated.LastRunAtUtc);
        // 미처리 구간이 남아 있으면 다음 예약을 현재 시각으로 설정해 이어서 처리합니다.
        Assert.Equal(now.AddMinutes(42), updated.NextRunAtUtc);
        Assert.False(await repository.RenewLeaseAsync(tenant.Id, firstLease,
            now.AddMinutes(13), now.AddMinutes(43)));
    }

    [Fact]
    public void MigrationSnapshotMatchesCurrentPlatformModel()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite("Data Source=:memory:").Options;
        using var db = new PlatformDbContext(options);
        var differ = db.GetService<IMigrationsModelDiffer>();
        var snapshot = new PlatformDbContextModelSnapshot().Model;
        var initializedSnapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot, designTime: true);
        var currentModel = db.GetService<IDesignTimeModel>().Model;
        var differences = differ.GetDifferences(initializedSnapshot.GetRelationalModel(), currentModel.GetRelationalModel());

        var descriptions = differences.Select(operation => operation switch
        {
            Microsoft.EntityFrameworkCore.Migrations.Operations.AlterColumnOperation alter =>
                $"열 변경: {alter.Table}.{alter.Name}, CLR={alter.ClrType.Name}, DB={alter.ColumnType}, Null={alter.IsNullable}, 이전 CLR={alter.OldColumn.ClrType.Name}, 이전 DB={alter.OldColumn.ColumnType}, 이전 Null={alter.OldColumn.IsNullable}",
            Microsoft.EntityFrameworkCore.Migrations.Operations.CreateIndexOperation index =>
                $"인덱스 추가: {index.Table}.{index.Name}, 열={string.Join(",", index.Columns)}, 고유={index.IsUnique}",
            _ => operation.GetType().Name
        });
        Assert.True(differences.Count == 0, string.Join(" | ", descriptions));
    }

    [Fact]
    public async Task SqlitePersistsScopedMembershipsAndEnforcesUniqueOwnership()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("SCOPE-A"), "범위 테스트", DateTimeOffset.UtcNow);
        db.Tenants.Add(tenant);
        var organization = Organization.Create(new OrganizationId(Guid.NewGuid()), tenant.Id, new OrganizationCode("ORG-A"), "조직 A");
        db.Organizations.Add(organization);
        var project = Project.Create(new ProjectId(Guid.NewGuid()), tenant.Id, organization.Id, new ProjectCode("PRJ-A"), "프로젝트 A");
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var repository = new ScopedMembershipRepository(db);
        repository.Add(OrganizationMembership.Grant(new OrganizationMembershipId(Guid.NewGuid()), tenant.Id, organization.Id,
            "scope-user", OrganizationRole.Contributor));
        repository.Add(ProjectMembership.Grant(new ProjectMembershipId(Guid.NewGuid()), tenant.Id, organization.Id, project.Id,
            "scope-user", ProjectRole.Reader));
        await db.SaveChangesAsync();

        Assert.True(await repository.HasOrganizationWriteAccessAsync(tenant.Id, organization.Id, "scope-user"));
        Assert.False(await repository.HasProjectWriteAccessAsync(tenant.Id, organization.Id, project.Id, "scope-user"));

        repository.Add(OrganizationMembership.Grant(new OrganizationMembershipId(Guid.NewGuid()), tenant.Id, organization.Id,
            "scope-user", OrganizationRole.Manager));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SqlitePersistsScopedAuthorizationPolicyAndAuditDecision()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("AUTH-A"), "권한 테스트 Tenant", DateTimeOffset.UtcNow);
        db.Tenants.Add(tenant);
        var organization = Organization.Create(new OrganizationId(Guid.NewGuid()), tenant.Id,
            new OrganizationCode("AUTH-ORG"), "권한 테스트 조직");
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();

        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenant.Id,
            organization.Id, null, "policy-subject", "resource:create", "DataSet",
            AuthorizationPolicyEffect.Deny, 500, DateTimeOffset.UtcNow);
        db.AuthorizationPolicies.Add(policy);
        var decision = new AuthorizationDecision(false, "policy.explicit_deny", policy.Id.Value.ToString());
        db.AuthorizationAuditEntries.Add(AuthorizationAuditEntry.Create(
            new AuthorizationAuditEntryId(Guid.NewGuid()), tenant.Id, "policy-subject",
            "resource:create", "DataSet", $"tenant:{tenant.Id.Value};organization:{organization.Id.Value};project:-",
            decision, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var repository = new AuthorizationPolicyRepository(db);
        var applicable = await repository.GetApplicableAsync(tenant.Id, organization.Id, null,
            "policy-subject", "resource:create", "DataSet");

        Assert.Single(applicable);
        Assert.Equal(AuthorizationPolicyEffect.Deny, applicable[0].Effect);
        Assert.Equal(1, await db.AuthorizationAuditEntries.CountAsync(x => !x.IsAllowed));
    }

    [Fact]
    public async Task SqlitePersistsPolicyChangeAuditWithActorAndBeforeAfterValues()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenant = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("CHANGE-A"), "정책 변경 Tenant", DateTimeOffset.UtcNow);
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var entry = AuthorizationPolicyChangeAuditEntry.Create(
            new AuthorizationPolicyChangeAuditEntryId(Guid.NewGuid()), tenant.Id, Guid.NewGuid(),
            "tenant-admin", "Updated", "{\"Effect\":\"Allow\",\"Priority\":10,\"IsEnabled\":true}",
            "{\"Effect\":\"Deny\",\"Priority\":100,\"IsEnabled\":true}", DateTimeOffset.UtcNow);
        db.AuthorizationPolicyChangeAuditEntries.Add(entry);
        await db.SaveChangesAsync();

        var persisted = await db.AuthorizationPolicyChangeAuditEntries.SingleAsync();
        Assert.Equal("tenant-admin", persisted.ActorSubjectId);
        Assert.Contains("Allow", persisted.BeforeJson, StringComparison.Ordinal);
        Assert.Contains("Deny", persisted.AfterJson, StringComparison.Ordinal);
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PlatformDbContext(options);
    }
}

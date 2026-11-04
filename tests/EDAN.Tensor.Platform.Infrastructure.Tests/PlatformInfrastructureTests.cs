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
    public void MigrationSnapshotMatchesCurrentPlatformModel()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite("Data Source=:memory:").Options;
        using var db = new PlatformDbContext(options);
        var differ = db.GetService<IMigrationsModelDiffer>();
        var snapshot = new PlatformDbContextModelSnapshot().Model;
        var currentModel = db.GetService<IDesignTimeModel>().Model;
        var differences = differ.GetDifferences(snapshot.GetRelationalModel(), currentModel.GetRelationalModel());

        Assert.True(differences.Count == 0,
            string.Join(", ", differences.Select(operation => operation.GetType().Name)));
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PlatformDbContext(options);
    }
}

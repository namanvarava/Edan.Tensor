using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        await db.Database.EnsureCreatedAsync();

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
        await db.Database.EnsureCreatedAsync();

        var firstTenant = new TenantId(Guid.NewGuid());
        var secondTenant = new TenantId(Guid.NewGuid());
        var email = new EmailAddress("shared@example.com");
        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), firstTenant, email, "첫 번째 사용자"));
        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), secondTenant, email, "두 번째 사용자"));
        await db.SaveChangesAsync();

        Assert.True(await new UserRepository(db).ExistsEmailAsync(firstTenant, email));
        Assert.True(await new UserRepository(db).ExistsEmailAsync(secondTenant, email));

        db.Users.Add(User.Invite(new UserId(Guid.NewGuid()), firstTenant, email, "중복 사용자"));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PlatformDbContext(options);
    }
}

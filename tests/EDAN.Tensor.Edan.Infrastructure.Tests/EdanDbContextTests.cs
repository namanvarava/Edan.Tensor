using EDAN.Tensor.Edan.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Edan.Infrastructure.Tests;

public sealed class EdanDbContextTests
{
    [Fact]
    public async Task SqliteMigrationCreatesSeparatedSysAndBasTables()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);

        await db.Database.MigrateAsync();

        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        await using var reader = await command.ExecuteReaderAsync();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));

        Assert.Contains("ed_00001", names);
        Assert.Contains("ed_00005", names);
        Assert.Contains("ed_01001", names);
        Assert.Contains("ed_01005", names);
    }

    [Fact]
    public async Task SqliteEnforcesFactoryToCompanyForeignKey()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.MigrateAsync();

        db.Factories.Add(new BasFactory
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Code = "FACTORY-INVALID",
            Name = "잘못된 공장"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static EdanDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<EdanDbContext>()
            .UseSqlite(connection)
            .Options;
        return new EdanDbContext(options);
    }
}

using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskQueueIndexTests
{
    [Fact]
    public async Task MigrationCreatesIndexForGlobalReadyTenantDiscovery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'index'
              AND name = 'IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId';
            """;

        var indexCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, indexCount);
    }
}

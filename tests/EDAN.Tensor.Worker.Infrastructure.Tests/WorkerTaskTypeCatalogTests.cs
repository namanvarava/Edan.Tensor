using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

/// <summary>작업 유형 카탈로그의 등록, 활성화 및 배포 동기화 경계를 검증합니다.</summary>
public sealed class WorkerTaskTypeCatalogTests
{
    [Fact]
    public async Task SynchronizationUpsertsRegisteredTypesWithoutDisablingUnlistedTypes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var catalog = new WorkerTaskTypeCatalog(db);
        var now = DateTimeOffset.UtcNow;
        await catalog.SynchronizeAsync(
        [
            new WorkerTaskTypeDefinition("dataset.validate", "DataSet 검증"),
            new WorkerTaskTypeDefinition("audit.archive", "감사 아카이브")
        ], now);

        Assert.True(await catalog.IsEnabledAsync("dataset.validate"));
        Assert.False(await catalog.IsEnabledAsync("unknown.task"));

        var audit = await db.WorkerTaskTypes.SingleAsync(x => x.TaskType == "audit.archive");
        audit.Disable(now.AddMinutes(1));
        await db.SaveChangesAsync();
        Assert.False(await catalog.IsEnabledAsync("audit.archive"));

        // 다른 버전에서 일부 유형만 등록해도 기존 유형을 자동 비활성화하지 않습니다.
        await catalog.SynchronizeAsync(
        [
            new WorkerTaskTypeDefinition("dataset.validate", "DataSet 검증 v2")
        ], now.AddMinutes(2));

        Assert.True(await catalog.IsEnabledAsync("dataset.validate"));
        Assert.False(await catalog.IsEnabledAsync("audit.archive"));
        Assert.Equal("DataSet 검증 v2",
            (await db.WorkerTaskTypes.SingleAsync(x => x.TaskType == "dataset.validate")).Description);
    }

    [Fact]
    public async Task SynchronizationRejectsDuplicateTaskTypes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var catalog = new WorkerTaskTypeCatalog(db);
        var definitions = new[]
        {
            new WorkerTaskTypeDefinition("duplicate.task"),
            new WorkerTaskTypeDefinition("duplicate.task")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalog.SynchronizeAsync(definitions, DateTimeOffset.UtcNow));
        Assert.Empty(await db.WorkerTaskTypes.ToListAsync());
    }
}

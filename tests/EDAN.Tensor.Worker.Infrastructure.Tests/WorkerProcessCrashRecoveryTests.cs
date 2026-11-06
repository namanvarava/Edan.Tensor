using System.Diagnostics;
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

/// <summary>
/// 테스트는 별도 프로세스가 저장한 Lease와 체크포인트가 강제 종료 후에도 보존되는지 검증합니다.
/// 시험은 실제 운영 WorkerHost와 외부 시스템 전체를 실행하지 않으며 Worker 저장소의 프로세스 장애 경계를 검증합니다.
/// </summary>
public sealed class WorkerProcessCrashRecoveryTests
{
    /// <summary>
    /// 테스트는 저장 완료 신호를 확인한 자식 프로세스를 강제 종료하고 새 DbContext에서 작업을 복구합니다.
    /// 테스트는 저장된 만료 시각 이후를 복구 시각으로 전달하여 벽시계 대기 없이 Lease 만료와 Fencing 조건을 검증합니다.
    /// </summary>
    [Fact]
    public async Task KilledProcessPreservesCheckpointAndRecoveryRejectsStaleAndCrossTenantWrites()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"edan-worker-crash-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "worker.db");
        var connection = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        Guid taskId;
        Process? child = null;
        Task<string>? errors = null;
        try
        {
            await using (var setup = new WorkerDbContext(options))
            {
                await setup.Database.MigrateAsync();
                var task = WorkerTask.Create(tenantId, "test.process-crash", "장애-복구-검증",
                    "payload://process-crash", DateTimeOffset.UtcNow, maxAttempts: 3);
                taskId = task.Id;
                setup.WorkerTasks.Add(task);
                await setup.SaveChangesAsync();
            }

            var probe = Path.Combine(AppContext.BaseDirectory, "ProcessProbe", "EDAN.Tensor.Worker.ProcessProbe.dll");
            Assert.True(File.Exists(probe), "빌드는 프로세스 장애 검증 프로그램을 테스트 출력 폴더에 복사해야 합니다.");
            var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            start.ArgumentList.Add(probe);
            start.ArgumentList.Add(databasePath);
            start.ArgumentList.Add(tenantId.ToString());
            start.ArgumentList.Add(taskId.ToString());
            child = Process.Start(start) ?? throw new InvalidOperationException("검증 프로세스가 시작되지 않았습니다.");
            errors = child.StandardError.ReadToEndAsync();
            var ready = await child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal("체크포인트-저장-완료", ready);

            Guid oldLeaseId;
            long oldToken;
            DateTimeOffset recoveryTime;
            await using (var beforeCrash = new WorkerDbContext(options))
            {
                var running = await new WorkerTaskRepository(beforeCrash).GetAsync(tenantId, taskId);
                Assert.NotNull(running);
                Assert.Equal("offset:42", running.Checkpoint);
                Assert.NotNull(running.LeaseId);
                oldLeaseId = running.LeaseId.Value;
                oldToken = running.FencingToken;
                recoveryTime = running.LeaseExpiresAtUtc!.Value.AddSeconds(1);
            }

            // 부모 프로세스는 정상 종료나 Dispose 경로를 호출하지 않고 자식 프로세스를 강제 종료합니다.
            child.Kill(entireProcessTree: true);
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.NotEqual(0, child.ExitCode);
            Assert.Equal(string.Empty, await errors.WaitAsync(TimeSpan.FromSeconds(5)));

            await using var recoveredDb = new WorkerDbContext(options);
            var repository = new WorkerTaskRepository(recoveredDb);
            Assert.Equal(1, await repository.RecoverExpiredLeasesAsync(recoveryTime, 10));
            var recovered = await repository.GetAsync(tenantId, taskId);
            Assert.NotNull(recovered);
            Assert.Equal(WorkerTaskStatus.RetryScheduled, recovered.Status);
            Assert.Equal("offset:42", recovered.Checkpoint);
            Assert.Null(await repository.GetAsync(otherTenantId, taskId));
            Assert.Null(await repository.TryAcquireLeaseAsync(
                otherTenantId, taskId, "다른-Tenant", Guid.NewGuid(), recoveryTime, recoveryTime.AddMinutes(1)));

            var newLeaseId = Guid.NewGuid();
            var acquired = await repository.TryAcquireLeaseAsync(
                tenantId, taskId, "복구-프로세스", newLeaseId, recoveryTime, recoveryTime.AddMinutes(1));
            Assert.NotNull(acquired);
            Assert.True(acquired.FencingToken > oldToken);
            Assert.Equal("offset:42", acquired.Checkpoint);
            Assert.False(await repository.TrySaveCheckpointAsync(
                tenantId, taskId, oldLeaseId, oldToken, "offset:오래된-쓰기", recoveryTime));
            Assert.False(await repository.TryCompleteAsync(tenantId, taskId, oldLeaseId, oldToken, recoveryTime));
            Assert.True(await repository.TryStartAsync(tenantId, taskId, newLeaseId, acquired.FencingToken, recoveryTime));
            Assert.True(await repository.TryCompleteAsync(
                tenantId, taskId, newLeaseId, acquired.FencingToken, recoveryTime, "result://recovered"));
            var completed = await repository.GetAsync(tenantId, taskId);
            Assert.Equal(WorkerTaskStatus.Succeeded, completed!.Status);
            Assert.Equal("offset:42", completed.Checkpoint);
        }
        finally
        {
            if (child is not null)
            {
                if (!child.HasExited)
                    child.Kill(entireProcessTree: true);
                await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
                child.Dispose();
            }

            // 연결 풀은 비활성화되어 있으므로 모든 DbContext와 자식 프로세스가 종료된 뒤 전용 DB를 제거할 수 있습니다.
            Directory.Delete(directory, recursive: true);
        }
    }
}

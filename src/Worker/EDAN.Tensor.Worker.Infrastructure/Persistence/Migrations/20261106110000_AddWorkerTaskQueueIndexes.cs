using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>SQLite에서 안전하게 비교할 UTC epoch 인덱스 열과 큐 조회 인덱스를 추가합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106110000_AddWorkerTaskQueueIndexes")]
public sealed class AddWorkerTaskQueueIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ReadyAtUtcUnixMilliseconds", table: "worker_tasks",
            type: "INTEGER", nullable: false, defaultValue: 0L);
        migrationBuilder.AddColumn<long>(
            name: "LeaseExpiresAtUtcUnixMilliseconds", table: "worker_tasks",
            type: "INTEGER", nullable: true);

        migrationBuilder.Sql("""
            UPDATE worker_tasks
            SET ReadyAtUtcUnixMilliseconds =
                CAST((julianday(COALESCE(NextAttemptAtUtc, CreatedAtUtc)) - 2440587.5) * 86400000 AS INTEGER);
            """);
        migrationBuilder.Sql("""
            UPDATE worker_tasks
            SET LeaseExpiresAtUtcUnixMilliseconds =
                CASE WHEN LeaseExpiresAtUtc IS NULL THEN NULL
                ELSE CAST((julianday(LeaseExpiresAtUtc) - 2440587.5) * 86400000 AS INTEGER) END;
            """);

        migrationBuilder.DropIndex(
            name: "IX_worker_tasks_TenantId_Status_NextAttemptAtUtc",
            table: "worker_tasks");
        migrationBuilder.CreateIndex(
            name: "IX_worker_tasks_TenantId_Status_ReadyAtUtcUnixMilliseconds",
            table: "worker_tasks",
            columns: new[] { "TenantId", "Status", "ReadyAtUtcUnixMilliseconds" });
        migrationBuilder.CreateIndex(
            name: "IX_worker_tasks_Status_LeaseExpiresAtUtcUnixMilliseconds",
            table: "worker_tasks",
            columns: new[] { "Status", "LeaseExpiresAtUtcUnixMilliseconds" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_worker_tasks_TenantId_Status_ReadyAtUtcUnixMilliseconds",
            table: "worker_tasks");
        migrationBuilder.DropIndex(
            name: "IX_worker_tasks_Status_LeaseExpiresAtUtcUnixMilliseconds",
            table: "worker_tasks");
        migrationBuilder.CreateIndex(
            name: "IX_worker_tasks_TenantId_Status_NextAttemptAtUtc",
            table: "worker_tasks",
            columns: new[] { "TenantId", "Status", "NextAttemptAtUtc" });
        migrationBuilder.DropColumn(name: "ReadyAtUtcUnixMilliseconds", table: "worker_tasks");
        migrationBuilder.DropColumn(name: "LeaseExpiresAtUtcUnixMilliseconds", table: "worker_tasks");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

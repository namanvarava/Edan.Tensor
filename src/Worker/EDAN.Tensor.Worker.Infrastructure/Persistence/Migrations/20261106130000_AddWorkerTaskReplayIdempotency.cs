using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>DLQ 재처리 요청의 멱등성 키를 저장하고 중복 재처리 작업 생성을 방지합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106130000_AddWorkerTaskReplayIdempotency")]
public sealed class AddWorkerTaskReplayIdempotency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ReplayRequestId", table: "worker_task_replays",
            type: "TEXT", nullable: false, defaultValue: Guid.Empty);
        migrationBuilder.CreateIndex(
            name: "IX_worker_task_replays_TenantId_OriginalTaskId_ReplayRequestId",
            table: "worker_task_replays",
            columns: new[] { "TenantId", "OriginalTaskId", "ReplayRequestId" },
            unique: true,
            filter: "\"ReplayRequestId\" <> '00000000-0000-0000-0000-000000000000'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_worker_task_replays_TenantId_OriginalTaskId_ReplayRequestId",
            table: "worker_task_replays");
        migrationBuilder.DropColumn(name: "ReplayRequestId", table: "worker_task_replays");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

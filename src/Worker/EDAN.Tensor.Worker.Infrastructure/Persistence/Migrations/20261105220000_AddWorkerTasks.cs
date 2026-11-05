using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>전용 Worker 작업 저장 테이블을 생성합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261105220000_AddWorkerTasks")]
public sealed class AddWorkerTasks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "worker_tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                TaskType = table.Column<string>(maxLength: 120, nullable: false),
                IdempotencyKey = table.Column<string>(maxLength: 200, nullable: false),
                PayloadReference = table.Column<string>(maxLength: 500, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false),
                AttemptCount = table.Column<int>(nullable: false),
                MaxAttempts = table.Column<int>(nullable: false),
                LeaseId = table.Column<Guid>(nullable: true),
                WorkerId = table.Column<string>(maxLength: 200, nullable: true),
                FencingToken = table.Column<long>(nullable: false),
                LeaseExpiresAtUtc = table.Column<DateTimeOffset>(nullable: true),
                Checkpoint = table.Column<string>(maxLength: 4000, nullable: true),
                CheckpointVersion = table.Column<long>(nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(nullable: true),
                NextAttemptAtUtc = table.Column<DateTimeOffset>(nullable: true),
                LastErrorCode = table.Column<string>(maxLength: 200, nullable: true),
                Version = table.Column<long>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_worker_tasks", x => x.Id));
        migrationBuilder.CreateIndex("IX_worker_tasks_TenantId_IdempotencyKey", "worker_tasks",
            new[] { "TenantId", "IdempotencyKey" }, unique: true);
        migrationBuilder.CreateIndex("IX_worker_tasks_TenantId_Status_NextAttemptAtUtc", "worker_tasks",
            new[] { "TenantId", "Status", "NextAttemptAtUtc" });
        migrationBuilder.CreateIndex("IX_worker_tasks_TenantId_LeaseId", "worker_tasks",
            new[] { "TenantId", "LeaseId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("worker_tasks");
}

using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>Tenant 범위의 영속 Worker 작업 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105210000_AddWorkerTasks")]
public sealed class AddWorkerTasks : Migration
{
    /// <summary>작업 상태, 임대, Fencing Token 및 체크포인트 저장 구조를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_worker_tasks",
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
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_worker_tasks", x => x.Id);
                table.ForeignKey("FK_platform_worker_tasks_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_platform_worker_tasks_TenantId_IdempotencyKey",
            table: "platform_worker_tasks",
            columns: new[] { "TenantId", "IdempotencyKey" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_platform_worker_tasks_TenantId_Status_NextAttemptAtUtc",
            table: "platform_worker_tasks",
            columns: new[] { "TenantId", "Status", "NextAttemptAtUtc" });
        migrationBuilder.CreateIndex(
            name: "IX_platform_worker_tasks_TenantId_LeaseId",
            table: "platform_worker_tasks",
            columns: new[] { "TenantId", "LeaseId" });
    }

    /// <summary>Worker 작업 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("platform_worker_tasks");

    /// <summary>현재 Migration의 기준 모델을 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}

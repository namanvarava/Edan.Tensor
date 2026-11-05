using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>Worker DLQ 재처리 요청 이력을 저장합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261105230000_AddWorkerTaskReplayHistory")]
public sealed class AddWorkerTaskReplayHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "worker_task_replays",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OriginalTaskId = table.Column<Guid>(nullable: false),
                ReplayTaskId = table.Column<Guid>(nullable: false),
                RequestedBy = table.Column<string>(maxLength: 200, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_worker_task_replays", x => x.Id));
        migrationBuilder.CreateIndex("IX_worker_task_replays_TenantId_OriginalTaskId_CreatedAtUtc",
            "worker_task_replays", new[] { "TenantId", "OriginalTaskId", "CreatedAtUtc" });
        migrationBuilder.CreateIndex("IX_worker_task_replays_TenantId_ReplayTaskId",
            "worker_task_replays", new[] { "TenantId", "ReplayTaskId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("worker_task_replays");

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

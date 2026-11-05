using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>작업 진행 상태와 완료 결과 참조를 영속화합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106090000_AddWorkerTaskProgressAndResult")]
public sealed class AddWorkerTaskProgressAndResult : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("ProgressPercent", "worker_tasks", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>("ProgressStage", "worker_tasks", maxLength: 120, nullable: true);
        migrationBuilder.AddColumn<string>("ProgressMessage", "worker_tasks", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("ResultReference", "worker_tasks", maxLength: 500, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ProgressPercent", "worker_tasks");
        migrationBuilder.DropColumn("ProgressStage", "worker_tasks");
        migrationBuilder.DropColumn("ProgressMessage", "worker_tasks");
        migrationBuilder.DropColumn("ResultReference", "worker_tasks");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

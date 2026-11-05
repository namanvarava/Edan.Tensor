using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>Worker가 실행할 수 있는 작업 유형 카탈로그를 영속화합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106100000_AddWorkerTaskTypeCatalog")]
public sealed class AddWorkerTaskTypeCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "worker_task_types",
            columns: table => new
            {
                TaskType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Version = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_worker_task_types", x => x.TaskType));
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "worker_task_types");

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

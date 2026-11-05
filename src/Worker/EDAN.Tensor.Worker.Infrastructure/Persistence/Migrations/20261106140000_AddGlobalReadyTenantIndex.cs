using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>전역 Ready Tenant 탐색이 준비된 작업만 스캔하도록 복합 인덱스를 추가합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106140000_AddGlobalReadyTenantIndex")]
public sealed class AddGlobalReadyTenantIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId",
            table: "worker_tasks",
            columns: new[] { "Status", "ReadyAtUtcUnixMilliseconds", "TenantId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId",
            table: "worker_tasks");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

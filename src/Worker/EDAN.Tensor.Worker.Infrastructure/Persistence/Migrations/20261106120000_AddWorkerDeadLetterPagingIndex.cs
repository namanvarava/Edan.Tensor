using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>DLQ 목록을 생성 시각 인덱스로 정렬하고 DB 측에서 페이징합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261106120000_AddWorkerDeadLetterPagingIndex")]
public sealed class AddWorkerDeadLetterPagingIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "CreatedAtUtcUnixMilliseconds", table: "worker_tasks",
            type: "INTEGER", nullable: false, defaultValue: 0L);
        migrationBuilder.Sql("""
            UPDATE worker_tasks
            SET CreatedAtUtcUnixMilliseconds =
                CAST((julianday(CreatedAtUtc) - 2440587.5) * 86400000 AS INTEGER);
            """);
        migrationBuilder.CreateIndex(
            name: "IX_worker_tasks_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id",
            table: "worker_tasks",
            columns: new[] { "TenantId", "Status", "CreatedAtUtcUnixMilliseconds", "Id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_worker_tasks_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id",
            table: "worker_tasks");
        migrationBuilder.DropColumn(name: "CreatedAtUtcUnixMilliseconds", table: "worker_tasks");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

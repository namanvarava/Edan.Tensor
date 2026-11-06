using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>외부 HTTP 전달의 Idempotency·재시도 상태를 추가합니다.</summary>
[DbContext(typeof(WorkerDbContext))]
[Migration("20261107100000_AddExternalDeliveries")]
public sealed class AddExternalDeliveries : Migration
{
    /// <summary>Worker SQLite에 외부 전달 상태 테이블과 Tenant 인덱스를 추가합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "external_deliveries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                TargetUri = table.Column<string>(type: "TEXT", maxLength: 2_048, nullable: false),
                Payload = table.Column<string>(type: "TEXT", maxLength: 1_048_576, nullable: false),
                IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                MaxAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                LastErrorCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Version = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_external_deliveries", x => x.Id));

        migrationBuilder.CreateIndex("IX_external_deliveries_TenantId_IdempotencyKey",
            "external_deliveries", new[] { "TenantId", "IdempotencyKey" }, unique: true);
        migrationBuilder.CreateIndex("IX_external_deliveries_TenantId_Status_CreatedAtUtc",
            "external_deliveries", new[] { "TenantId", "Status", "CreatedAtUtc" });
    }

    /// <summary>외부 전달 상태 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("external_deliveries");

    /// <summary>마이그레이션 대상 모델은 실행 모델과 동일하게 구성됩니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}

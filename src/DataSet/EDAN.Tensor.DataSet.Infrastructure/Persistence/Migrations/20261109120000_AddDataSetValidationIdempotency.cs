using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>DataSet 검증 실행에 Tenant 범위 Idempotency-Key를 추가합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("20261109120000_AddDataSetValidationIdempotency")]
public sealed class AddDataSetValidationIdempotency : Migration
{
    /// <summary>검증 실행의 Idempotency-Key 열과 Tenant 범위 고유 인덱스를 추가합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "IdempotencyKey",
            table: "dataset_validation_executions",
            type: "TEXT",
            maxLength: 200,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
            table: "dataset_validation_executions",
            columns: new[] { "TenantId", "IdempotencyKey" },
            unique: true);
    }

    /// <summary>검증 실행의 Idempotency-Key 열과 고유 인덱스를 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
            table: "dataset_validation_executions");

        migrationBuilder.DropColumn(
            name: "IdempotencyKey",
            table: "dataset_validation_executions");
    }

    /// <summary>현재 DataSet 저장 모델을 마이그레이션 대상 모델로 사용합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}

using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>DataSet 검증 결과에 Worker Fencing Token을 추가하여 오래된 실행의 쓰기를 차단합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("20261107120000_AddDataSetValidationFencing")]
public sealed class AddDataSetValidationFencing : Migration
{
    /// <summary>검증 실행에 현재 Worker Fencing Token 열을 추가합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "FencingToken",
            table: "dataset_validation_executions",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0L);
    }

    /// <summary>검증 실행의 Worker Fencing Token 열을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FencingToken",
            table: "dataset_validation_executions");
    }

    /// <summary>현재 DataSet 저장 모델을 마이그레이션 대상 모델로 사용합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}

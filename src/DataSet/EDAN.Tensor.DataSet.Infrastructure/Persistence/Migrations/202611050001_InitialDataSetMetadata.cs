using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>DataSet 메타데이터와 불변 스키마 이력의 초기 저장 구조를 생성합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("202611050001_InitialDataSetMetadata")]
public sealed class InitialDataSetMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "datasets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                StorageAdapter = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Version = table.Column<int>(type: "INTEGER", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                StorageReference = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_datasets", x => x.Id));

        migrationBuilder.CreateTable(
            name: "dataset_schema_revisions",
            columns: table => new
            {
                DataSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                Version = table.Column<int>(type: "INTEGER", nullable: false),
                SchemaJson = table.Column<string>(type: "TEXT", maxLength: 1048576, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_dataset_schema_revisions", x => new { x.DataSetId, x.Version });
                table.ForeignKey("FK_dataset_schema_revisions_datasets_DataSetId", x => x.DataSetId,
                    "datasets", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_datasets_Status_CreatedAtUtc", "datasets", new[] { "Status", "CreatedAtUtc" });
        migrationBuilder.CreateIndex("IX_datasets_TenantId_OrganizationId_ProjectId", "datasets",
            new[] { "TenantId", "OrganizationId", "ProjectId" });
        migrationBuilder.CreateIndex("IX_dataset_schema_revisions_DataSetId", "dataset_schema_revisions", "DataSetId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("dataset_schema_revisions");
        migrationBuilder.DropTable("datasets");
    }

    /// <summary>마이그레이션이 생성하는 모델을 현재 런타임 모델과 동일하게 고정합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}

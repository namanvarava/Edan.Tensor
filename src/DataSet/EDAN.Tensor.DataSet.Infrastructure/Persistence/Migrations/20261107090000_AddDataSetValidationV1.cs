using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>DataSet 콘텐츠 형식과 Tenant 범위의 검증 프로필·실행 결과 저장소를 추가합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("20261107090000_AddDataSetValidationV1")]
public sealed class AddDataSetValidationV1 : Migration
{
    /// <summary>SQLite에 승인된 DataSet Validation v1 구조를 추가합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ContentFormat",
            table: "datasets",
            type: "TEXT",
            maxLength: 24,
            nullable: false,
            defaultValue: "json-array");

        migrationBuilder.CreateTable(
            name: "dataset_validation_profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_validation_profiles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "dataset_validation_profile_revisions",
            columns: table => new
            {
                ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                Revision = table.Column<int>(type: "INTEGER", nullable: false),
                DefinitionJson = table.Column<string>(type: "TEXT", maxLength: 1_048_576, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_validation_profile_revisions", x => new { x.ProfileId, x.Revision }));

        migrationBuilder.CreateTable(
            name: "dataset_validation_executions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                DataSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                ExpectedDataSetVersion = table.Column<int>(type: "INTEGER", nullable: false),
                SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                ProfileRevision = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                DetailReference = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                ProcessedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                ErrorCount = table.Column<int>(type: "INTEGER", nullable: false),
                WarningCount = table.Column<int>(type: "INTEGER", nullable: false),
                DetailItemCount = table.Column<int>(type: "INTEGER", nullable: false),
                DetailTruncated = table.Column<bool>(type: "INTEGER", nullable: false),
                RuleCountsJson = table.Column<string>(type: "TEXT", maxLength: 1_048_576, nullable: false),
                FailureCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                FailureMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Version = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_validation_executions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "dataset_validation_details",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                ExecutionId = table.Column<Guid>(type: "TEXT", nullable: false),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                DetailReference = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                FieldName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                Code = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Severity = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_validation_details", x => x.Id));

        migrationBuilder.CreateTable(
            name: "dataset_validation_seen_keys",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                ExecutionId = table.Column<Guid>(type: "TEXT", nullable: false),
                RuleCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                KeyHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_validation_seen_keys", x => x.Id));

        migrationBuilder.CreateIndex("IX_dataset_validation_profiles_TenantId_Name",
            "dataset_validation_profiles", new[] { "TenantId", "Name" }, unique: true);
        migrationBuilder.CreateIndex("IX_dataset_validation_profile_revisions_TenantId_ProfileId_Revision",
            "dataset_validation_profile_revisions", new[] { "TenantId", "ProfileId", "Revision" });
        migrationBuilder.CreateIndex("IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtUtc",
            "dataset_validation_executions", new[] { "TenantId", "DataSetId", "CreatedAtUtc" });
        migrationBuilder.CreateIndex("IX_dataset_validation_executions_TenantId_DetailReference",
            "dataset_validation_executions", new[] { "TenantId", "DetailReference" }, unique: true);
        migrationBuilder.CreateIndex("IX_dataset_validation_details_TenantId_ExecutionId_RowNumber",
            "dataset_validation_details", new[] { "TenantId", "ExecutionId", "RowNumber" });
        migrationBuilder.CreateIndex("IX_dataset_validation_details_ExecutionId_RowNumber_Code_FieldName",
            "dataset_validation_details", new[] { "ExecutionId", "RowNumber", "Code", "FieldName" }, unique: true);
        migrationBuilder.CreateIndex("IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash",
            "dataset_validation_seen_keys", new[] { "ExecutionId", "RuleCode", "KeyHash" }, unique: true);
    }

    /// <summary>검증 v1 구조와 콘텐츠 형식 열을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("dataset_validation_seen_keys");
        migrationBuilder.DropTable("dataset_validation_details");
        migrationBuilder.DropTable("dataset_validation_executions");
        migrationBuilder.DropTable("dataset_validation_profile_revisions");
        migrationBuilder.DropTable("dataset_validation_profiles");
        migrationBuilder.DropColumn("ContentFormat", "datasets");
    }

    /// <summary>마이그레이션 대상 모델은 실행 모델과 동일하게 구성됩니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}

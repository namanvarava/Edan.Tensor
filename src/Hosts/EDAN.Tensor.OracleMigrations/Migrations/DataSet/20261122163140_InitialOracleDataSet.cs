using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class InitialOracleDataSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dataset_quota_threshold_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PolicyId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Scope = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    TargetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventType = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    UsedBytes = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    LimitBytes = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    UtilizationPercent = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    PolicyVersion = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_quota_threshold_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_storage_usage_snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PeriodStartUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UsedBytes = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DataSetCount = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_storage_usage_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_validation_details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ExecutionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DetailReference = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    RowNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FieldName = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    Code = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    Message = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_validation_details", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_validation_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DataSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ExpectedDataSetVersion = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SchemaVersion = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProfileId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProfileRevision = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FencingToken = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    DetailReference = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    ProcessedRecords = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TotalRecords = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ErrorCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    WarningCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DetailItemCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DetailTruncated = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    RuleCountsJson = table.Column<string>(type: "NCLOB", maxLength: 1048576, nullable: false),
                    FailureCode = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FailureMessage = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_validation_executions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_validation_profile_revisions",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Revision = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DefinitionJson = table.Column<string>(type: "NCLOB", maxLength: 1048576, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_validation_profile_revisions", x => new { x.ProfileId, x.Revision });
                });

            migrationBuilder.CreateTable(
                name: "dataset_validation_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_validation_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_validation_seen_keys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ExecutionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RuleCode = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    KeyHash = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_validation_seen_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "datasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    StorageReference = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    StorageAdapter = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    ContentFormat = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_schema_revisions",
                columns: table => new
                {
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DataSetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SchemaJson = table.Column<string>(type: "NCLOB", maxLength: 1048576, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dataset_schema_revisions", x => new { x.DataSetId, x.Version });
                    table.ForeignKey(
                        name: "FK_dataset_schema_revisions_datasets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc",
                table: "dataset_quota_threshold_events",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAtUtc",
                table: "dataset_quota_threshold_events",
                columns: new[] { "TenantId", "PolicyId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc",
                table: "dataset_storage_usage_snapshots",
                columns: new[] { "TenantId", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc",
                table: "dataset_storage_usage_snapshots",
                columns: new[] { "TenantId", "PeriodStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_FieldName",
                table: "dataset_validation_details",
                columns: new[] { "ExecutionId", "RowNumber", "Code", "FieldName" },
                unique: true,
                filter: "\"FieldName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber",
                table: "dataset_validation_details",
                columns: new[] { "TenantId", "ExecutionId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtUtc",
                table: "dataset_validation_executions",
                columns: new[] { "TenantId", "DataSetId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_executions_TenantId_DetailReference",
                table: "dataset_validation_executions",
                columns: new[] { "TenantId", "DetailReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
                table: "dataset_validation_executions",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Revision",
                table: "dataset_validation_profile_revisions",
                columns: new[] { "TenantId", "ProfileId", "Revision" });

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_profiles_TenantId_Name",
                table: "dataset_validation_profiles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash",
                table: "dataset_validation_seen_keys",
                columns: new[] { "ExecutionId", "RuleCode", "KeyHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_datasets_Status_CreatedAtUtc",
                table: "datasets",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_datasets_TenantId_OrganizationId_ProjectId",
                table: "datasets",
                columns: new[] { "TenantId", "OrganizationId", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dataset_quota_threshold_events");

            migrationBuilder.DropTable(
                name: "dataset_schema_revisions");

            migrationBuilder.DropTable(
                name: "dataset_storage_usage_snapshots");

            migrationBuilder.DropTable(
                name: "dataset_validation_details");

            migrationBuilder.DropTable(
                name: "dataset_validation_executions");

            migrationBuilder.DropTable(
                name: "dataset_validation_profile_revisions");

            migrationBuilder.DropTable(
                name: "dataset_validation_profiles");

            migrationBuilder.DropTable(
                name: "dataset_validation_seen_keys");

            migrationBuilder.DropTable(
                name: "datasets");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EDAN.Tensor.ApiHost.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedTableNamesPg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dataset_schema_revisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_datasets",
                table: "datasets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_validation_seen_keys",
                table: "dataset_validation_seen_keys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_validation_profiles",
                table: "dataset_validation_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_validation_profile_revisions",
                table: "dataset_validation_profile_revisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_validation_executions",
                table: "dataset_validation_executions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_validation_details",
                table: "dataset_validation_details");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_storage_usage_snapshots",
                table: "dataset_storage_usage_snapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset_quota_threshold_events",
                table: "dataset_quota_threshold_events");

            migrationBuilder.RenameTable(
                name: "datasets",
                newName: "edds_dset");

            migrationBuilder.RenameTable(
                name: "dataset_validation_seen_keys",
                newName: "edds_dsetvld_seenkey");

            migrationBuilder.RenameTable(
                name: "dataset_validation_profiles",
                newName: "edds_dsetvld_prf");

            migrationBuilder.RenameTable(
                name: "dataset_validation_profile_revisions",
                newName: "edds_dsetvld_prfrev");

            migrationBuilder.RenameTable(
                name: "dataset_validation_executions",
                newName: "edds_dsetvld_exc");

            migrationBuilder.RenameTable(
                name: "dataset_validation_details",
                newName: "edds_dsetvld_dtl");

            migrationBuilder.RenameTable(
                name: "dataset_storage_usage_snapshots",
                newName: "edds_dsetstr_usgsnp");

            migrationBuilder.RenameTable(
                name: "dataset_quota_threshold_events",
                newName: "edds_dsetqta_threvt");

            migrationBuilder.RenameIndex(
                name: "IX_datasets_TenantId_OrganizationId_ProjectId",
                table: "edds_dset",
                newName: "IX_edds_dset_TenantId_OrganizationId_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_datasets_Status_CreatedAtUtc",
                table: "edds_dset",
                newName: "IX_edds_dset_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash",
                table: "edds_dsetvld_seenkey",
                newName: "IX_edds_dsetvld_seenkey_ExecutionId_RuleCode_KeyHash");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profiles_TenantId_Name",
                table: "edds_dsetvld_prf",
                newName: "IX_edds_dsetvld_prf_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Rev~",
                table: "edds_dsetvld_prfrev",
                newName: "IX_edds_dsetvld_prfrev_TenantId_ProfileId_Revision");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DetailReference",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_DetailReference");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtU~",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_DataSetId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber",
                table: "edds_dsetvld_dtl",
                newName: "IX_edds_dsetvld_dtl_TenantId_ExecutionId_RowNumber");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_Field~",
                table: "edds_dsetvld_dtl",
                newName: "IX_edds_dsetvld_dtl_ExecutionId_RowNumber_Code_FieldName");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "IX_edds_dsetstr_usgsnp_TenantId_PeriodStartUtc");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "IX_edds_dsetstr_usgsnp_TenantId_CapturedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAt~",
                table: "edds_dsetqta_threvt",
                newName: "IX_edds_dsetqta_threvt_TenantId_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "IX_edds_dsetqta_threvt_TenantId_OccurredAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dset",
                table: "edds_dset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_seenkey",
                table: "edds_dsetvld_seenkey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_prf",
                table: "edds_dsetvld_prf",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_prfrev",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "ProfileId", "Revision" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_exc",
                table: "edds_dsetvld_exc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_dtl",
                table: "edds_dsetvld_dtl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetstr_usgsnp",
                table: "edds_dsetstr_usgsnp",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetqta_threvt",
                table: "edds_dsetqta_threvt",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "edds_dsetschm_rev",
                columns: table => new
                {
                    Version = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaJson = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edds_dsetschm_rev", x => new { x.DataSetId, x.Version });
                    table.ForeignKey(
                        name: "FK_edds_dsetschm_rev_edds_dset_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "edds_dset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edds_dsetschm_rev");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_seenkey",
                table: "edds_dsetvld_seenkey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_prfrev",
                table: "edds_dsetvld_prfrev");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_prf",
                table: "edds_dsetvld_prf");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_exc",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_dtl",
                table: "edds_dsetvld_dtl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetstr_usgsnp",
                table: "edds_dsetstr_usgsnp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetqta_threvt",
                table: "edds_dsetqta_threvt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dset",
                table: "edds_dset");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_seenkey",
                newName: "dataset_validation_seen_keys");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_prfrev",
                newName: "dataset_validation_profile_revisions");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_prf",
                newName: "dataset_validation_profiles");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_exc",
                newName: "dataset_validation_executions");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_dtl",
                newName: "dataset_validation_details");

            migrationBuilder.RenameTable(
                name: "edds_dsetstr_usgsnp",
                newName: "dataset_storage_usage_snapshots");

            migrationBuilder.RenameTable(
                name: "edds_dsetqta_threvt",
                newName: "dataset_quota_threshold_events");

            migrationBuilder.RenameTable(
                name: "edds_dset",
                newName: "datasets");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_seenkey_ExecutionId_RuleCode_KeyHash",
                table: "dataset_validation_seen_keys",
                newName: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_prfrev_TenantId_ProfileId_Revision",
                table: "dataset_validation_profile_revisions",
                newName: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Rev~");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_prf_TenantId_Name",
                table: "dataset_validation_profiles",
                newName: "IX_dataset_validation_profiles_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_IdempotencyKey",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_DetailReference",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DetailReference");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_DataSetId_CreatedAtUtc",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtU~");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_dtl_TenantId_ExecutionId_RowNumber",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_dtl_ExecutionId_RowNumber_Code_FieldName",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_Field~");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetstr_usgsnp_TenantId_PeriodStartUtc",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetstr_usgsnp_TenantId_CapturedAtUtc",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetqta_threvt_TenantId_PolicyId_OccurredAtUtc",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAt~");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetqta_threvt_TenantId_OccurredAtUtc",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dset_TenantId_OrganizationId_ProjectId",
                table: "datasets",
                newName: "IX_datasets_TenantId_OrganizationId_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dset_Status_CreatedAtUtc",
                table: "datasets",
                newName: "IX_datasets_Status_CreatedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_validation_seen_keys",
                table: "dataset_validation_seen_keys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_validation_profile_revisions",
                table: "dataset_validation_profile_revisions",
                columns: new[] { "ProfileId", "Revision" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_validation_profiles",
                table: "dataset_validation_profiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_validation_executions",
                table: "dataset_validation_executions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_validation_details",
                table: "dataset_validation_details",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_storage_usage_snapshots",
                table: "dataset_storage_usage_snapshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset_quota_threshold_events",
                table: "dataset_quota_threshold_events",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_datasets",
                table: "datasets",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "dataset_schema_revisions",
                columns: table => new
                {
                    DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SchemaJson = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false)
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
        }
    }
}

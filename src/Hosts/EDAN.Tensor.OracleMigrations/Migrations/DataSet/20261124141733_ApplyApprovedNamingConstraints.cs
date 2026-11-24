using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "dataset_schema_revisions",
                newName: "edds_dsetschm_rev");

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
                newName: "edds_dset_N02");

            migrationBuilder.RenameIndex(
                name: "IX_datasets_Status_CreatedAtUtc",
                table: "edds_dset",
                newName: "edds_dset_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash",
                table: "edds_dsetvld_seenkey",
                newName: "edds_dsetvld_seenkey_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profiles_TenantId_Name",
                table: "edds_dsetvld_prf",
                newName: "edds_dsetvld_prf_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Revision",
                table: "edds_dsetvld_prfrev",
                newName: "edds_dsetvld_prfrev_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DetailReference",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtUtc",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N03");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_FieldName",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "ProfileId", "Revision" });

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt",
                column: "Id");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "edds_dsetschm_rev",
                newName: "dataset_schema_revisions");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dset_PK",
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
                name: "edds_dsetvld_seenkey_N01",
                table: "dataset_validation_seen_keys",
                newName: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prfrev_N01",
                table: "dataset_validation_profile_revisions",
                newName: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Revision");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prf_N01",
                table: "dataset_validation_profiles",
                newName: "IX_dataset_validation_profiles_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N03",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N02",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N01",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DetailReference");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_FieldName");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N01",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N02",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N01",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N02",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N01",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N02",
                table: "datasets",
                newName: "IX_datasets_TenantId_OrganizationId_ProjectId");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N01",
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

            migrationBuilder.RenameTable(
                name: "edds_dsetschm_rev",
                newName: "dataset_schema_revisions");
        }
    }
}

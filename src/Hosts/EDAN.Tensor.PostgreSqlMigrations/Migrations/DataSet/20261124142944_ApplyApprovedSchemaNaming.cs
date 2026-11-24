using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class ApplyApprovedSchemaNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_dataset_schema_revisions_datasets_DataSetId", "dataset_schema_revisions");
            migrationBuilder.DropPrimaryKey("PK_dataset_schema_revisions", "dataset_schema_revisions");
            migrationBuilder.RenameTable("dataset_schema_revisions", "edds_dsetschm_rev");
            migrationBuilder.RenameColumn("DataSetId", "edds_dsetschm_rev", "dsetid");
            migrationBuilder.RenameColumn("Version", "edds_dsetschm_rev", "ver");
            migrationBuilder.RenameColumn("SchemaJson", "edds_dsetschm_rev", "schm_json");
            migrationBuilder.RenameColumn("CreatedAtUtc", "edds_dsetschm_rev", "crt_utc");

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

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edds_dset",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dset",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "StorageReference",
                table: "edds_dset",
                newName: "str_ref");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edds_dset",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "edds_dset",
                newName: "prjid");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "edds_dset",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "edds_dset",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edds_dset",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dset",
                newName: "dsetid");

            migrationBuilder.RenameIndex(
                name: "IX_datasets_TenantId_OrganizationId_ProjectId",
                table: "edds_dset",
                newName: "edds_dset_N02");

            migrationBuilder.RenameIndex(
                name: "IX_datasets_Status_CreatedAtUtc",
                table: "edds_dset",
                newName: "edds_dset_N01");

            migrationBuilder.RenameColumn(
                name: "RuleCode",
                table: "edds_dsetvld_seenkey",
                newName: "rule_cd");

            migrationBuilder.RenameColumn(
                name: "KeyHash",
                table: "edds_dsetvld_seenkey",
                newName: "key_hash");

            migrationBuilder.RenameColumn(
                name: "ExecutionId",
                table: "edds_dsetvld_seenkey",
                newName: "excid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetvld_seenkey",
                newName: "dsetvldseenkeyid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash",
                table: "edds_dsetvld_seenkey",
                newName: "edds_dsetvld_seenkey_N01");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetvld_prf",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "edds_dsetvld_prf",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edds_dsetvld_prf",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetvld_prf",
                newName: "dsetvldprfid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profiles_TenantId_Name",
                table: "edds_dsetvld_prf",
                newName: "edds_dsetvld_prf_N01");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetvld_prfrev",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edds_dsetvld_prfrev",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Revision",
                table: "edds_dsetvld_prfrev",
                newName: "rev");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "edds_dsetvld_prfrev",
                newName: "prfid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Rev~",
                table: "edds_dsetvld_prfrev",
                newName: "edds_dsetvld_prfrev_N01");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edds_dsetvld_exc",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetvld_exc",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edds_dsetvld_exc",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "edds_dsetvld_exc",
                newName: "prfid");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "edds_dsetvld_exc",
                newName: "idem_key");

            migrationBuilder.RenameColumn(
                name: "FencingToken",
                table: "edds_dsetvld_exc",
                newName: "fence_tkn");

            migrationBuilder.RenameColumn(
                name: "DetailReference",
                table: "edds_dsetvld_exc",
                newName: "dtl_ref");

            migrationBuilder.RenameColumn(
                name: "DataSetId",
                table: "edds_dsetvld_exc",
                newName: "dsetid");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edds_dsetvld_exc",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "edds_dsetvld_exc",
                newName: "cmp_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetvld_exc",
                newName: "dsetvldexcid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_IdempotencyKey",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DetailReference",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtU~",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N03");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetvld_dtl",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Severity",
                table: "edds_dsetvld_dtl",
                newName: "sev");

            migrationBuilder.RenameColumn(
                name: "RowNumber",
                table: "edds_dsetvld_dtl",
                newName: "row_no");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "edds_dsetvld_dtl",
                newName: "msg");

            migrationBuilder.RenameColumn(
                name: "FieldName",
                table: "edds_dsetvld_dtl",
                newName: "fld_nm");

            migrationBuilder.RenameColumn(
                name: "ExecutionId",
                table: "edds_dsetvld_dtl",
                newName: "excid");

            migrationBuilder.RenameColumn(
                name: "DetailReference",
                table: "edds_dsetvld_dtl",
                newName: "dtl_ref");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "edds_dsetvld_dtl",
                newName: "cd");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetvld_dtl",
                newName: "dsetvlddtlid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N01");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_Field~",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N02");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetstr_usgsnp",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "CapturedAtUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "capt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetstr_usgsnp",
                newName: "dsetstrusgsnpid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N01");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edds_dsetqta_threvt",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "edds_dsetqta_threvt",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edds_dsetqta_threvt",
                newName: "dsetqtathrevtid");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAt~",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N02");

            migrationBuilder.RenameIndex(
                name: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset",
                column: "dsetid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey",
                column: "dsetvldseenkeyid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf",
                column: "dsetvldprfid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "prfid", "rev" });

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc",
                column: "dsetvldexcid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl",
                column: "dsetvlddtlid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp",
                column: "dsetstrusgsnpid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt",
                column: "dsetqtathrevtid");

            migrationBuilder.AddPrimaryKey("edds_dsetschm_rev_PK", "edds_dsetschm_rev", new[] { "dsetid", "ver" });
            migrationBuilder.AddForeignKey("edds_dsetschm_rev_FKDataSetId", "edds_dsetschm_rev", "dsetid", "edds_dset", "dsetid", onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("edds_dsetschm_rev_FKDataSetId", "edds_dsetschm_rev");
            migrationBuilder.DropPrimaryKey("edds_dsetschm_rev_PK", "edds_dsetschm_rev");
            migrationBuilder.RenameColumn("dsetid", "edds_dsetschm_rev", "DataSetId");
            migrationBuilder.RenameColumn("ver", "edds_dsetschm_rev", "Version");
            migrationBuilder.RenameColumn("schm_json", "edds_dsetschm_rev", "SchemaJson");
            migrationBuilder.RenameColumn("crt_utc", "edds_dsetschm_rev", "CreatedAtUtc");
            migrationBuilder.RenameTable("edds_dsetschm_rev", "dataset_schema_revisions");
            migrationBuilder.AddPrimaryKey("PK_dataset_schema_revisions", "dataset_schema_revisions", new[] { "DataSetId", "Version" });
            migrationBuilder.AddForeignKey("FK_dataset_schema_revisions_datasets_DataSetId", "dataset_schema_revisions", "DataSetId", "datasets", "Id", onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.RenameColumn(
                name: "rule_cd",
                table: "dataset_validation_seen_keys",
                newName: "RuleCode");

            migrationBuilder.RenameColumn(
                name: "key_hash",
                table: "dataset_validation_seen_keys",
                newName: "KeyHash");

            migrationBuilder.RenameColumn(
                name: "excid",
                table: "dataset_validation_seen_keys",
                newName: "ExecutionId");

            migrationBuilder.RenameColumn(
                name: "dsetvldseenkeyid",
                table: "dataset_validation_seen_keys",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_seenkey_N01",
                table: "dataset_validation_seen_keys",
                newName: "IX_dataset_validation_seen_keys_ExecutionId_RuleCode_KeyHash");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_validation_profile_revisions",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "dataset_validation_profile_revisions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "rev",
                table: "dataset_validation_profile_revisions",
                newName: "Revision");

            migrationBuilder.RenameColumn(
                name: "prfid",
                table: "dataset_validation_profile_revisions",
                newName: "ProfileId");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prfrev_N01",
                table: "dataset_validation_profile_revisions",
                newName: "IX_dataset_validation_profile_revisions_TenantId_ProfileId_Rev~");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_validation_profiles",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "dataset_validation_profiles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "dataset_validation_profiles",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetvldprfid",
                table: "dataset_validation_profiles",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prf_N01",
                table: "dataset_validation_profiles",
                newName: "IX_dataset_validation_profiles_TenantId_Name");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "dataset_validation_executions",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_validation_executions",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "dataset_validation_executions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prfid",
                table: "dataset_validation_executions",
                newName: "ProfileId");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "dataset_validation_executions",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "fence_tkn",
                table: "dataset_validation_executions",
                newName: "FencingToken");

            migrationBuilder.RenameColumn(
                name: "dtl_ref",
                table: "dataset_validation_executions",
                newName: "DetailReference");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "dataset_validation_executions",
                newName: "DataSetId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "dataset_validation_executions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "dataset_validation_executions",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetvldexcid",
                table: "dataset_validation_executions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N03",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DataSetId_CreatedAtU~");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N02",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N01",
                table: "dataset_validation_executions",
                newName: "IX_dataset_validation_executions_TenantId_DetailReference");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_validation_details",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "sev",
                table: "dataset_validation_details",
                newName: "Severity");

            migrationBuilder.RenameColumn(
                name: "row_no",
                table: "dataset_validation_details",
                newName: "RowNumber");

            migrationBuilder.RenameColumn(
                name: "msg",
                table: "dataset_validation_details",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "fld_nm",
                table: "dataset_validation_details",
                newName: "FieldName");

            migrationBuilder.RenameColumn(
                name: "excid",
                table: "dataset_validation_details",
                newName: "ExecutionId");

            migrationBuilder.RenameColumn(
                name: "dtl_ref",
                table: "dataset_validation_details",
                newName: "DetailReference");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "dataset_validation_details",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "dsetvlddtlid",
                table: "dataset_validation_details",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_ExecutionId_RowNumber_Code_Field~");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N01",
                table: "dataset_validation_details",
                newName: "IX_dataset_validation_details_TenantId_ExecutionId_RowNumber");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_storage_usage_snapshots",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "capt_utc",
                table: "dataset_storage_usage_snapshots",
                newName: "CapturedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetstrusgsnpid",
                table: "dataset_storage_usage_snapshots",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N02",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N01",
                table: "dataset_storage_usage_snapshots",
                newName: "IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "dataset_quota_threshold_events",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "dataset_quota_threshold_events",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "dataset_quota_threshold_events",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetqtathrevtid",
                table: "dataset_quota_threshold_events",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N02",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAt~");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N01",
                table: "dataset_quota_threshold_events",
                newName: "IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "datasets",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "datasets",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "str_ref",
                table: "datasets",
                newName: "StorageReference");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "datasets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "datasets",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "datasets",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "datasets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "datasets",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "datasets",
                newName: "Id");

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

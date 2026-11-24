using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class ApplyApprovedColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl");

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

            migrationBuilder.RenameColumn(
                name: "SchemaJson",
                table: "edds_dsetschm_rev",
                newName: "schm_json");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edds_dsetschm_rev",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edds_dsetschm_rev",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "DataSetId",
                table: "edds_dsetschm_rev",
                newName: "dsetid");

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

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edds_dsetvld_exc",
                columns: new[] { "tntid", "idem_key" },
                unique: true,
                filter: "[idem_key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl",
                columns: new[] { "excid", "row_no", "cd", "fld_nm" },
                unique: true,
                filter: "[fld_nm] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl");

            migrationBuilder.RenameColumn(
                name: "rule_cd",
                table: "edds_dsetvld_seenkey",
                newName: "RuleCode");

            migrationBuilder.RenameColumn(
                name: "key_hash",
                table: "edds_dsetvld_seenkey",
                newName: "KeyHash");

            migrationBuilder.RenameColumn(
                name: "excid",
                table: "edds_dsetvld_seenkey",
                newName: "ExecutionId");

            migrationBuilder.RenameColumn(
                name: "dsetvldseenkeyid",
                table: "edds_dsetvld_seenkey",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetvld_prfrev",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edds_dsetvld_prfrev",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "rev",
                table: "edds_dsetvld_prfrev",
                newName: "Revision");

            migrationBuilder.RenameColumn(
                name: "prfid",
                table: "edds_dsetvld_prfrev",
                newName: "ProfileId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetvld_prf",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edds_dsetvld_prf",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edds_dsetvld_prf",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetvldprfid",
                table: "edds_dsetvld_prf",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edds_dsetvld_exc",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetvld_exc",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edds_dsetvld_exc",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prfid",
                table: "edds_dsetvld_exc",
                newName: "ProfileId");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "edds_dsetvld_exc",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "fence_tkn",
                table: "edds_dsetvld_exc",
                newName: "FencingToken");

            migrationBuilder.RenameColumn(
                name: "dtl_ref",
                table: "edds_dsetvld_exc",
                newName: "DetailReference");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "edds_dsetvld_exc",
                newName: "DataSetId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edds_dsetvld_exc",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edds_dsetvld_exc",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetvldexcid",
                table: "edds_dsetvld_exc",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetvld_dtl",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "sev",
                table: "edds_dsetvld_dtl",
                newName: "Severity");

            migrationBuilder.RenameColumn(
                name: "row_no",
                table: "edds_dsetvld_dtl",
                newName: "RowNumber");

            migrationBuilder.RenameColumn(
                name: "msg",
                table: "edds_dsetvld_dtl",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "fld_nm",
                table: "edds_dsetvld_dtl",
                newName: "FieldName");

            migrationBuilder.RenameColumn(
                name: "excid",
                table: "edds_dsetvld_dtl",
                newName: "ExecutionId");

            migrationBuilder.RenameColumn(
                name: "dtl_ref",
                table: "edds_dsetvld_dtl",
                newName: "DetailReference");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edds_dsetvld_dtl",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "dsetvlddtlid",
                table: "edds_dsetvld_dtl",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetstr_usgsnp",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "capt_utc",
                table: "edds_dsetstr_usgsnp",
                newName: "CapturedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetstrusgsnpid",
                table: "edds_dsetstr_usgsnp",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "schm_json",
                table: "edds_dsetschm_rev",
                newName: "SchemaJson");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edds_dsetschm_rev",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edds_dsetschm_rev",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "edds_dsetschm_rev",
                newName: "DataSetId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dsetqta_threvt",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edds_dsetqta_threvt",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edds_dsetqta_threvt",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetqtathrevtid",
                table: "edds_dsetqta_threvt",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edds_dset",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edds_dset",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "str_ref",
                table: "edds_dset",
                newName: "StorageReference");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edds_dset",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edds_dset",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edds_dset",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edds_dset",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edds_dset",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "edds_dset",
                newName: "Id");

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edds_dsetvld_exc",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl",
                columns: new[] { "ExecutionId", "RowNumber", "Code", "FieldName" },
                unique: true,
                filter: "[FieldName] IS NOT NULL");
        }
    }
}

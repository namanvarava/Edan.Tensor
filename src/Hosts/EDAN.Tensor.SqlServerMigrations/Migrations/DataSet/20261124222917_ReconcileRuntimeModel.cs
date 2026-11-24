using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class ReconcileRuntimeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl");

            migrationBuilder.RenameColumn(
                name: "dsetvldseenkeyid",
                table: "edds_dsetvld_seenkey",
                newName: "dset_vld_seen_keyid");

            migrationBuilder.RenameColumn(
                name: "DefinitionJson",
                table: "edds_dsetvld_prfrev",
                newName: "def_json");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edds_dsetvld_prf",
                newName: "dset_vld_prfnm");

            migrationBuilder.RenameColumn(
                name: "dsetvldprfid",
                table: "edds_dsetvld_prf",
                newName: "dset_vld_prfid");

            migrationBuilder.RenameColumn(
                name: "WarningCount",
                table: "edds_dsetvld_exc",
                newName: "warn_cnt");

            migrationBuilder.RenameColumn(
                name: "TotalRecords",
                table: "edds_dsetvld_exc",
                newName: "tot_rec_cnt");

            migrationBuilder.RenameColumn(
                name: "SchemaVersion",
                table: "edds_dsetvld_exc",
                newName: "schm_ver");

            migrationBuilder.RenameColumn(
                name: "RuleCountsJson",
                table: "edds_dsetvld_exc",
                newName: "rule_cnt_json");

            migrationBuilder.RenameColumn(
                name: "ProfileRevision",
                table: "edds_dsetvld_exc",
                newName: "prf_rev");

            migrationBuilder.RenameColumn(
                name: "ProcessedRecords",
                table: "edds_dsetvld_exc",
                newName: "proc_rec_cnt");

            migrationBuilder.RenameColumn(
                name: "FailureMessage",
                table: "edds_dsetvld_exc",
                newName: "fail_msg");

            migrationBuilder.RenameColumn(
                name: "FailureCode",
                table: "edds_dsetvld_exc",
                newName: "fail_cd");

            migrationBuilder.RenameColumn(
                name: "ExpectedDataSetVersion",
                table: "edds_dsetvld_exc",
                newName: "exp_dset_ver");

            migrationBuilder.RenameColumn(
                name: "ErrorCount",
                table: "edds_dsetvld_exc",
                newName: "err_cnt");

            migrationBuilder.RenameColumn(
                name: "DetailTruncated",
                table: "edds_dsetvld_exc",
                newName: "dtl_trunc_yn");

            migrationBuilder.RenameColumn(
                name: "DetailItemCount",
                table: "edds_dsetvld_exc",
                newName: "dtl_item_cnt");

            migrationBuilder.RenameColumn(
                name: "dsetvldexcid",
                table: "edds_dsetvld_exc",
                newName: "dset_vld_excid");

            migrationBuilder.RenameColumn(
                name: "row_no",
                table: "edds_dsetvld_dtl",
                newName: "row_num");

            migrationBuilder.RenameColumn(
                name: "fld_nm",
                table: "edds_dsetvld_dtl",
                newName: "fldnm");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edds_dsetvld_dtl",
                newName: "dset_vld_dtlcd");

            migrationBuilder.RenameColumn(
                name: "dsetvlddtlid",
                table: "edds_dsetvld_dtl",
                newName: "dset_vld_dtlid");

            migrationBuilder.RenameColumn(
                name: "capt_utc",
                table: "edds_dsetstr_usgsnp",
                newName: "cap_utc");

            migrationBuilder.RenameColumn(
                name: "UsedBytes",
                table: "edds_dsetstr_usgsnp",
                newName: "used_bytes");

            migrationBuilder.RenameColumn(
                name: "PeriodStartUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "period_strt_utc");

            migrationBuilder.RenameColumn(
                name: "DataSetCount",
                table: "edds_dsetstr_usgsnp",
                newName: "data_set_count");

            migrationBuilder.RenameColumn(
                name: "dsetstrusgsnpid",
                table: "edds_dsetstr_usgsnp",
                newName: "dset_str_usg_snpid");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edds_dsetqta_threvt",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "UtilizationPercent",
                table: "edds_dsetqta_threvt",
                newName: "utilization_percent");

            migrationBuilder.RenameColumn(
                name: "UsedBytes",
                table: "edds_dsetqta_threvt",
                newName: "used_bytes");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "edds_dsetqta_threvt",
                newName: "tgtid");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "edds_dsetqta_threvt",
                newName: "scp");

            migrationBuilder.RenameColumn(
                name: "PolicyVersion",
                table: "edds_dsetqta_threvt",
                newName: "policy_version");

            migrationBuilder.RenameColumn(
                name: "LimitBytes",
                table: "edds_dsetqta_threvt",
                newName: "limit_bytes");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "edds_dsetqta_threvt",
                newName: "event_type");

            migrationBuilder.RenameColumn(
                name: "dsetqtathrevtid",
                table: "edds_dsetqta_threvt",
                newName: "dset_qta_thr_evtid");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edds_dset",
                newName: "dsetnm");

            migrationBuilder.RenameColumn(
                name: "StorageAdapter",
                table: "edds_dset",
                newName: "str_adp");

            migrationBuilder.RenameColumn(
                name: "ContentFormat",
                table: "edds_dset",
                newName: "ctt_fmt");

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl",
                columns: new[] { "excid", "row_num", "dset_vld_dtlcd", "fldnm" },
                unique: true,
                filter: "[fldnm] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl");

            migrationBuilder.RenameColumn(
                name: "dset_vld_seen_keyid",
                table: "edds_dsetvld_seenkey",
                newName: "dsetvldseenkeyid");

            migrationBuilder.RenameColumn(
                name: "def_json",
                table: "edds_dsetvld_prfrev",
                newName: "DefinitionJson");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfnm",
                table: "edds_dsetvld_prf",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfid",
                table: "edds_dsetvld_prf",
                newName: "dsetvldprfid");

            migrationBuilder.RenameColumn(
                name: "warn_cnt",
                table: "edds_dsetvld_exc",
                newName: "WarningCount");

            migrationBuilder.RenameColumn(
                name: "tot_rec_cnt",
                table: "edds_dsetvld_exc",
                newName: "TotalRecords");

            migrationBuilder.RenameColumn(
                name: "schm_ver",
                table: "edds_dsetvld_exc",
                newName: "SchemaVersion");

            migrationBuilder.RenameColumn(
                name: "rule_cnt_json",
                table: "edds_dsetvld_exc",
                newName: "RuleCountsJson");

            migrationBuilder.RenameColumn(
                name: "proc_rec_cnt",
                table: "edds_dsetvld_exc",
                newName: "ProcessedRecords");

            migrationBuilder.RenameColumn(
                name: "prf_rev",
                table: "edds_dsetvld_exc",
                newName: "ProfileRevision");

            migrationBuilder.RenameColumn(
                name: "fail_msg",
                table: "edds_dsetvld_exc",
                newName: "FailureMessage");

            migrationBuilder.RenameColumn(
                name: "fail_cd",
                table: "edds_dsetvld_exc",
                newName: "FailureCode");

            migrationBuilder.RenameColumn(
                name: "exp_dset_ver",
                table: "edds_dsetvld_exc",
                newName: "ExpectedDataSetVersion");

            migrationBuilder.RenameColumn(
                name: "err_cnt",
                table: "edds_dsetvld_exc",
                newName: "ErrorCount");

            migrationBuilder.RenameColumn(
                name: "dtl_trunc_yn",
                table: "edds_dsetvld_exc",
                newName: "DetailTruncated");

            migrationBuilder.RenameColumn(
                name: "dtl_item_cnt",
                table: "edds_dsetvld_exc",
                newName: "DetailItemCount");

            migrationBuilder.RenameColumn(
                name: "dset_vld_excid",
                table: "edds_dsetvld_exc",
                newName: "dsetvldexcid");

            migrationBuilder.RenameColumn(
                name: "row_num",
                table: "edds_dsetvld_dtl",
                newName: "row_no");

            migrationBuilder.RenameColumn(
                name: "fldnm",
                table: "edds_dsetvld_dtl",
                newName: "fld_nm");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlcd",
                table: "edds_dsetvld_dtl",
                newName: "cd");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlid",
                table: "edds_dsetvld_dtl",
                newName: "dsetvlddtlid");

            migrationBuilder.RenameColumn(
                name: "used_bytes",
                table: "edds_dsetstr_usgsnp",
                newName: "UsedBytes");

            migrationBuilder.RenameColumn(
                name: "period_strt_utc",
                table: "edds_dsetstr_usgsnp",
                newName: "PeriodStartUtc");

            migrationBuilder.RenameColumn(
                name: "data_set_count",
                table: "edds_dsetstr_usgsnp",
                newName: "DataSetCount");

            migrationBuilder.RenameColumn(
                name: "cap_utc",
                table: "edds_dsetstr_usgsnp",
                newName: "capt_utc");

            migrationBuilder.RenameColumn(
                name: "dset_str_usg_snpid",
                table: "edds_dsetstr_usgsnp",
                newName: "dsetstrusgsnpid");

            migrationBuilder.RenameColumn(
                name: "utilization_percent",
                table: "edds_dsetqta_threvt",
                newName: "UtilizationPercent");

            migrationBuilder.RenameColumn(
                name: "used_bytes",
                table: "edds_dsetqta_threvt",
                newName: "UsedBytes");

            migrationBuilder.RenameColumn(
                name: "tgtid",
                table: "edds_dsetqta_threvt",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "scp",
                table: "edds_dsetqta_threvt",
                newName: "Scope");

            migrationBuilder.RenameColumn(
                name: "policy_version",
                table: "edds_dsetqta_threvt",
                newName: "PolicyVersion");

            migrationBuilder.RenameColumn(
                name: "occ_utc",
                table: "edds_dsetqta_threvt",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "limit_bytes",
                table: "edds_dsetqta_threvt",
                newName: "LimitBytes");

            migrationBuilder.RenameColumn(
                name: "event_type",
                table: "edds_dsetqta_threvt",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "dset_qta_thr_evtid",
                table: "edds_dsetqta_threvt",
                newName: "dsetqtathrevtid");

            migrationBuilder.RenameColumn(
                name: "str_adp",
                table: "edds_dset",
                newName: "StorageAdapter");

            migrationBuilder.RenameColumn(
                name: "dsetnm",
                table: "edds_dset",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "ctt_fmt",
                table: "edds_dset",
                newName: "ContentFormat");

            migrationBuilder.CreateIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl",
                columns: new[] { "excid", "row_no", "cd", "fld_nm" },
                unique: true,
                filter: "[fld_nm] IS NOT NULL");
        }
    }
}

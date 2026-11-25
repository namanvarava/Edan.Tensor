using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ReconcileRuntimeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE edpf_strqta_plcy DROP CONSTRAINT IF EXISTS \"AK_edpf_strqta_plcy_strqtaplcyid_tntid\";");

            migrationBuilder.Sql(
                "ALTER TABLE edpf_authaud_arcjob DROP CONSTRAINT IF EXISTS \"AK_edpf_authaud_arcjob_authaudarcjobid_tntid\";");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tntrtnalwid",
                table: "edpf_tntrtnalw",
                newName: "tnt_rtn_alwid");

            migrationBuilder.Sql("DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = 'edpf_tntmut_aud' AND column_name = 'tntmutaudid') AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = 'edpf_tntmut_aud' AND column_name = 'tnt_mut_audid') THEN ALTER TABLE edpf_tntmut_aud RENAME COLUMN tntmutaudid TO tnt_mut_audid; END IF; END $$;");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tntmbrid",
                table: "edpf_tntmbr",
                newName: "tnt_mbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tntdbndid",
                table: "edpf_tntdbnd",
                newName: "tnt_dbndid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nm",
                table: "edpf_tnt",
                newName: "tntnm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "cd",
                table: "edpf_tnt",
                newName: "tntcd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occr_utc",
                table: "edpf_strqta_plcychg",
                newName: "occ_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "strqtaplcychgid",
                table: "edpf_strqta_plcychg",
                newName: "str_qta_plcy_chgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "WarningThresholdPercent",
                table: "edpf_strqta_plcy",
                newName: "warn_thr_pct");

            RenameColumnIfNeeded(migrationBuilder,
                name: "UpdatedBy",
                table: "edpf_strqta_plcy",
                newName: "upd_subid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "TenantLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "tnt_lmt_byt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "TargetId",
                table: "edpf_strqta_plcy",
                newName: "tgtid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Scope",
                table: "edpf_strqta_plcy",
                newName: "scp");

            RenameColumnIfNeeded(migrationBuilder,
                name: "ProjectLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "prj_lmt_byt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "PlanCode",
                table: "edpf_strqta_plcy",
                newName: "plan_cd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "OrganizationLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "org_lmt_byt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "DataSetLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "dset_lmt_byt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "strqtaplcyid",
                table: "edpf_strqta_plcy",
                newName: "str_qta_plcyid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nm",
                table: "edpf_rsrc",
                newName: "rsrcnm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "prjmbrid",
                table: "edpf_prjmbr",
                newName: "prj_mbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nm",
                table: "edpf_prj",
                newName: "prjnm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "cd",
                table: "edpf_prj",
                newName: "prjcd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Value",
                table: "edpf_plcy",
                newName: "val");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Key",
                table: "edpf_plcy",
                newName: "plcykey");

            RenameColumnIfNeeded(migrationBuilder,
                name: "orgmbrid",
                table: "edpf_orgmbr",
                newName: "org_mbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nm",
                table: "edpf_org",
                newName: "orgnm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "cd",
                table: "edpf_org",
                newName: "orgcd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occr_utc",
                table: "edpf_authplcy_chg",
                newName: "occ_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authplcychgid",
                table: "edpf_authplcy_chg",
                newName: "auth_plcy_chgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Priority",
                table: "edpf_authplcy",
                newName: "pri");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Effect",
                table: "edpf_authplcy",
                newName: "eff");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Action",
                table: "edpf_authplcy",
                newName: "act");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authplcyid",
                table: "edpf_authplcy",
                newName: "auth_plcyid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occr_utc",
                table: "edpf_authaud_rtnchg",
                newName: "occ_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudrtnchgid",
                table: "edpf_authaud_rtnchg",
                newName: "auth_aud_rtn_chgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "rtn_days",
                table: "edpf_authaud_rtn",
                newName: "rtn_day");

            RenameColumnIfNeeded(migrationBuilder,
                name: "UpdatedBy",
                table: "edpf_authaud_rtn",
                newName: "upd_subid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "LegalHoldReason",
                table: "edpf_authaud_rtn",
                newName: "legal_hold_rsn");

            RenameColumnIfNeeded(migrationBuilder,
                name: "IsLegalHold",
                table: "edpf_authaud_rtn",
                newName: "legal_hold_yn");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudrtnid",
                table: "edpf_authaud_rtn",
                newName: "auth_aud_rtnid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "LeaseId",
                table: "edpf_authaud_arcschd",
                newName: "leaseid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nxt_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "nxt_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "last_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "last_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "IntervalHours",
                table: "edpf_authaud_arcschd",
                newName: "int_hr");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudarcschdid",
                table: "edpf_authaud_arcschd",
                newName: "auth_aud_arc_schdid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Sha256",
                table: "edpf_authaud_arcjob",
                newName: "sha256");

            RenameColumnIfNeeded(migrationBuilder,
                name: "ToUtc",
                table: "edpf_authaud_arcjob",
                newName: "to_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "SizeBytes",
                table: "edpf_authaud_arcjob",
                newName: "size_byt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "RecordCount",
                table: "edpf_authaud_arcjob",
                newName: "rec_cnt");

            RenameColumnIfNeeded(migrationBuilder,
                name: "FromUtc",
                table: "edpf_authaud_arcjob",
                newName: "from_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "FailureReason",
                table: "edpf_authaud_arcjob",
                newName: "fail_rsn");

            RenameColumnIfNeeded(migrationBuilder,
                name: "ArchiveId",
                table: "edpf_authaud_arcjob",
                newName: "arcid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudarcjobid",
                table: "edpf_authaud_arcjob",
                newName: "auth_aud_arc_jobid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "fnsh_utc",
                table: "edpf_authaud_arcattm",
                newName: "fin_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "ResultCode",
                table: "edpf_authaud_arcattm",
                newName: "result_code");

            RenameColumnIfNeeded(migrationBuilder,
                name: "IsSuccessful",
                table: "edpf_authaud_arcattm",
                newName: "succ_yn");

            RenameColumnIfNeeded(migrationBuilder,
                name: "AttemptNumber",
                table: "edpf_authaud_arcattm",
                newName: "attm_num");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudarcattmid",
                table: "edpf_authaud_arcattm",
                newName: "auth_aud_arc_attmid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occr_utc",
                table: "edpf_authaud",
                newName: "occ_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Scope",
                table: "edpf_authaud",
                newName: "scp");

            RenameColumnIfNeeded(migrationBuilder,
                name: "ReasonCode",
                table: "edpf_authaud",
                newName: "rsn_cd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "MatchedPolicyId",
                table: "edpf_authaud",
                newName: "plcyid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "IsAllowed",
                table: "edpf_authaud",
                newName: "allow_yn");

            RenameColumnIfNeeded(migrationBuilder,
                name: "Action",
                table: "edpf_authaud",
                newName: "act");

            RenameColumnIfNeeded(migrationBuilder,
                name: "authaudid",
                table: "edpf_authaud",
                newName: "auth_audid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_str_qta_plcyid_tntid",
                table: "edpf_strqta_plcy",
                columns: new[] { "str_qta_plcyid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid",
                table: "edpf_authaud_arcjob",
                columns: new[] { "auth_aud_arc_jobid", "tntid" });
        }

        /// <inheritdoc />        private static void RenameColumnIfNeeded(MigrationBuilder migrationBuilder, string name, string table, string newName)
        {
            migrationBuilder.Sql($@"DO $
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = '{table}' AND column_name = '{name}')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = '{table}' AND column_name = '{newName}') THEN
        EXECUTE format('ALTER TABLE %I RENAME COLUMN %I TO %I', '{table}', '{name}', '{newName}');
    END IF;
END $;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_str_qta_plcyid_tntid",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid",
                table: "edpf_authaud_arcjob");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tnt_rtn_alwid",
                table: "edpf_tntrtnalw",
                newName: "tntrtnalwid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tnt_mut_audid",
                table: "edpf_tntmut_aud",
                newName: "tntmutaudid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tnt_mbrid",
                table: "edpf_tntmbr",
                newName: "tntmbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tnt_dbndid",
                table: "edpf_tntdbnd",
                newName: "tntdbndid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tntnm",
                table: "edpf_tnt",
                newName: "nm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tntcd",
                table: "edpf_tnt",
                newName: "cd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occ_utc",
                table: "edpf_strqta_plcychg",
                newName: "occr_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "str_qta_plcy_chgid",
                table: "edpf_strqta_plcychg",
                newName: "strqtaplcychgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "warn_thr_pct",
                table: "edpf_strqta_plcy",
                newName: "WarningThresholdPercent");

            RenameColumnIfNeeded(migrationBuilder,
                name: "upd_subid",
                table: "edpf_strqta_plcy",
                newName: "UpdatedBy");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tnt_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "TenantLimitBytes");

            RenameColumnIfNeeded(migrationBuilder,
                name: "tgtid",
                table: "edpf_strqta_plcy",
                newName: "TargetId");

            RenameColumnIfNeeded(migrationBuilder,
                name: "scp",
                table: "edpf_strqta_plcy",
                newName: "Scope");

            RenameColumnIfNeeded(migrationBuilder,
                name: "prj_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "ProjectLimitBytes");

            RenameColumnIfNeeded(migrationBuilder,
                name: "plan_cd",
                table: "edpf_strqta_plcy",
                newName: "PlanCode");

            RenameColumnIfNeeded(migrationBuilder,
                name: "org_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "OrganizationLimitBytes");

            RenameColumnIfNeeded(migrationBuilder,
                name: "dset_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "DataSetLimitBytes");

            RenameColumnIfNeeded(migrationBuilder,
                name: "str_qta_plcyid",
                table: "edpf_strqta_plcy",
                newName: "strqtaplcyid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "rsrcnm",
                table: "edpf_rsrc",
                newName: "nm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "prj_mbrid",
                table: "edpf_prjmbr",
                newName: "prjmbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "prjnm",
                table: "edpf_prj",
                newName: "nm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "prjcd",
                table: "edpf_prj",
                newName: "cd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "val",
                table: "edpf_plcy",
                newName: "Value");

            RenameColumnIfNeeded(migrationBuilder,
                name: "plcykey",
                table: "edpf_plcy",
                newName: "Key");

            RenameColumnIfNeeded(migrationBuilder,
                name: "org_mbrid",
                table: "edpf_orgmbr",
                newName: "orgmbrid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "orgnm",
                table: "edpf_org",
                newName: "nm");

            RenameColumnIfNeeded(migrationBuilder,
                name: "orgcd",
                table: "edpf_org",
                newName: "cd");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occ_utc",
                table: "edpf_authplcy_chg",
                newName: "occr_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_plcy_chgid",
                table: "edpf_authplcy_chg",
                newName: "authplcychgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "pri",
                table: "edpf_authplcy",
                newName: "Priority");

            RenameColumnIfNeeded(migrationBuilder,
                name: "eff",
                table: "edpf_authplcy",
                newName: "Effect");

            RenameColumnIfNeeded(migrationBuilder,
                name: "act",
                table: "edpf_authplcy",
                newName: "Action");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_plcyid",
                table: "edpf_authplcy",
                newName: "authplcyid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occ_utc",
                table: "edpf_authaud_rtnchg",
                newName: "occr_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_aud_rtn_chgid",
                table: "edpf_authaud_rtnchg",
                newName: "authaudrtnchgid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "upd_subid",
                table: "edpf_authaud_rtn",
                newName: "UpdatedBy");

            RenameColumnIfNeeded(migrationBuilder,
                name: "rtn_day",
                table: "edpf_authaud_rtn",
                newName: "rtn_days");

            RenameColumnIfNeeded(migrationBuilder,
                name: "legal_hold_yn",
                table: "edpf_authaud_rtn",
                newName: "IsLegalHold");

            RenameColumnIfNeeded(migrationBuilder,
                name: "legal_hold_rsn",
                table: "edpf_authaud_rtn",
                newName: "LegalHoldReason");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_aud_rtnid",
                table: "edpf_authaud_rtn",
                newName: "authaudrtnid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "leaseid",
                table: "edpf_authaud_arcschd",
                newName: "LeaseId");

            RenameColumnIfNeeded(migrationBuilder,
                name: "nxt_utc",
                table: "edpf_authaud_arcschd",
                newName: "nxt_run_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "last_utc",
                table: "edpf_authaud_arcschd",
                newName: "last_run_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "int_hr",
                table: "edpf_authaud_arcschd",
                newName: "IntervalHours");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_aud_arc_schdid",
                table: "edpf_authaud_arcschd",
                newName: "authaudarcschdid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "sha256",
                table: "edpf_authaud_arcjob",
                newName: "Sha256");

            RenameColumnIfNeeded(migrationBuilder,
                name: "to_utc",
                table: "edpf_authaud_arcjob",
                newName: "ToUtc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "size_byt",
                table: "edpf_authaud_arcjob",
                newName: "SizeBytes");

            RenameColumnIfNeeded(migrationBuilder,
                name: "rec_cnt",
                table: "edpf_authaud_arcjob",
                newName: "RecordCount");

            RenameColumnIfNeeded(migrationBuilder,
                name: "from_utc",
                table: "edpf_authaud_arcjob",
                newName: "FromUtc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "fail_rsn",
                table: "edpf_authaud_arcjob",
                newName: "FailureReason");

            RenameColumnIfNeeded(migrationBuilder,
                name: "arcid",
                table: "edpf_authaud_arcjob",
                newName: "ArchiveId");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_aud_arc_jobid",
                table: "edpf_authaud_arcjob",
                newName: "authaudarcjobid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "succ_yn",
                table: "edpf_authaud_arcattm",
                newName: "IsSuccessful");

            RenameColumnIfNeeded(migrationBuilder,
                name: "result_code",
                table: "edpf_authaud_arcattm",
                newName: "ResultCode");

            RenameColumnIfNeeded(migrationBuilder,
                name: "fin_utc",
                table: "edpf_authaud_arcattm",
                newName: "fnsh_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "attm_num",
                table: "edpf_authaud_arcattm",
                newName: "AttemptNumber");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_aud_arc_attmid",
                table: "edpf_authaud_arcattm",
                newName: "authaudarcattmid");

            RenameColumnIfNeeded(migrationBuilder,
                name: "scp",
                table: "edpf_authaud",
                newName: "Scope");

            RenameColumnIfNeeded(migrationBuilder,
                name: "rsn_cd",
                table: "edpf_authaud",
                newName: "ReasonCode");

            RenameColumnIfNeeded(migrationBuilder,
                name: "plcyid",
                table: "edpf_authaud",
                newName: "MatchedPolicyId");

            RenameColumnIfNeeded(migrationBuilder,
                name: "occ_utc",
                table: "edpf_authaud",
                newName: "occr_utc");

            RenameColumnIfNeeded(migrationBuilder,
                name: "allow_yn",
                table: "edpf_authaud",
                newName: "IsAllowed");

            RenameColumnIfNeeded(migrationBuilder,
                name: "act",
                table: "edpf_authaud",
                newName: "Action");

            RenameColumnIfNeeded(migrationBuilder,
                name: "auth_audid",
                table: "edpf_authaud",
                newName: "authaudid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_strqtaplcyid_tntid",
                table: "edpf_strqta_plcy",
                columns: new[] { "strqtaplcyid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_authaudarcjobid_tntid",
                table: "edpf_authaud_arcjob",
                columns: new[] { "authaudarcjobid", "tntid" });
        }
    }
}

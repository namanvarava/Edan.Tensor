using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ReconcileRuntimeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The approved naming migrations already applied these changes. This
            // generated reconciliation migration would rename columns that are
            // already in their final form on existing SQL Server databases.
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                return;

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_strqta_plcy_strqtaplcyid_tntid' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_strqta_plcy]')) ALTER TABLE [dbo].[edpf_strqta_plcy] DROP CONSTRAINT [AK_edpf_strqta_plcy_strqtaplcyid_tntid];");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_authaud_arcjob_authaudarcjobid_tntid' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_authaud_arcjob]')) ALTER TABLE [dbo].[edpf_authaud_arcjob] DROP CONSTRAINT [AK_edpf_authaud_arcjob_authaudarcjobid_tntid];");

            migrationBuilder.RenameColumn(
                name: "tntrtnalwid",
                table: "edpf_tntrtnalw",
                newName: "tnt_rtn_alwid");

            migrationBuilder.Sql("IF COL_LENGTH(N'dbo.edpf_tntmut_aud', N'tntmutaudid') IS NOT NULL AND COL_LENGTH(N'dbo.edpf_tntmut_aud', N'tnt_mut_audid') IS NULL EXEC sp_rename N'dbo.edpf_tntmut_aud.tntmutaudid', N'tnt_mut_audid', N'COLUMN';");

            migrationBuilder.RenameColumn(
                name: "tntmbrid",
                table: "edpf_tntmbr",
                newName: "tnt_mbrid");

            migrationBuilder.RenameColumn(
                name: "tntdbndid",
                table: "edpf_tntdbnd",
                newName: "tnt_dbndid");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_tnt",
                newName: "tntnm");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_tnt",
                newName: "tntcd");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_strqta_plcychg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "strqtaplcychgid",
                table: "edpf_strqta_plcychg",
                newName: "str_qta_plcy_chgid");

            migrationBuilder.RenameColumn(
                name: "WarningThresholdPercent",
                table: "edpf_strqta_plcy",
                newName: "warn_thr_pct");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "edpf_strqta_plcy",
                newName: "upd_subid");

            migrationBuilder.RenameColumn(
                name: "TenantLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "tnt_lmt_byt");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "edpf_strqta_plcy",
                newName: "tgtid");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "edpf_strqta_plcy",
                newName: "scp");

            migrationBuilder.RenameColumn(
                name: "ProjectLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "prj_lmt_byt");

            migrationBuilder.RenameColumn(
                name: "PlanCode",
                table: "edpf_strqta_plcy",
                newName: "plan_cd");

            migrationBuilder.RenameColumn(
                name: "OrganizationLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "org_lmt_byt");

            migrationBuilder.RenameColumn(
                name: "DataSetLimitBytes",
                table: "edpf_strqta_plcy",
                newName: "dset_lmt_byt");

            migrationBuilder.RenameColumn(
                name: "strqtaplcyid",
                table: "edpf_strqta_plcy",
                newName: "str_qta_plcyid");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_rsrc",
                newName: "rsrcnm");

            migrationBuilder.RenameColumn(
                name: "prjmbrid",
                table: "edpf_prjmbr",
                newName: "prj_mbrid");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_prj",
                newName: "prjnm");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_prj",
                newName: "prjcd");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "edpf_plcy",
                newName: "val");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "edpf_plcy",
                newName: "plcykey");

            migrationBuilder.RenameColumn(
                name: "orgmbrid",
                table: "edpf_orgmbr",
                newName: "org_mbrid");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_org",
                newName: "orgnm");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_org",
                newName: "orgcd");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authplcy_chg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "authplcychgid",
                table: "edpf_authplcy_chg",
                newName: "auth_plcy_chgid");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "edpf_authplcy",
                newName: "pri");

            migrationBuilder.RenameColumn(
                name: "Effect",
                table: "edpf_authplcy",
                newName: "eff");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "edpf_authplcy",
                newName: "act");

            migrationBuilder.RenameColumn(
                name: "authplcyid",
                table: "edpf_authplcy",
                newName: "auth_plcyid");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud_rtnchg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "authaudrtnchgid",
                table: "edpf_authaud_rtnchg",
                newName: "auth_aud_rtn_chgid");

            migrationBuilder.RenameColumn(
                name: "rtn_days",
                table: "edpf_authaud_rtn",
                newName: "rtn_day");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "edpf_authaud_rtn",
                newName: "upd_subid");

            migrationBuilder.RenameColumn(
                name: "LegalHoldReason",
                table: "edpf_authaud_rtn",
                newName: "legal_hold_rsn");

            migrationBuilder.RenameColumn(
                name: "IsLegalHold",
                table: "edpf_authaud_rtn",
                newName: "legal_hold_yn");

            migrationBuilder.RenameColumn(
                name: "authaudrtnid",
                table: "edpf_authaud_rtn",
                newName: "auth_aud_rtnid");

            migrationBuilder.RenameColumn(
                name: "LeaseId",
                table: "edpf_authaud_arcschd",
                newName: "leaseid");

            migrationBuilder.RenameColumn(
                name: "nxt_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "nxt_utc");

            migrationBuilder.RenameColumn(
                name: "last_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "last_utc");

            migrationBuilder.RenameColumn(
                name: "IntervalHours",
                table: "edpf_authaud_arcschd",
                newName: "int_hr");

            migrationBuilder.RenameColumn(
                name: "authaudarcschdid",
                table: "edpf_authaud_arcschd",
                newName: "auth_aud_arc_schdid");

            migrationBuilder.RenameColumn(
                name: "Sha256",
                table: "edpf_authaud_arcjob",
                newName: "sha256");

            migrationBuilder.RenameColumn(
                name: "ToUtc",
                table: "edpf_authaud_arcjob",
                newName: "to_utc");

            migrationBuilder.RenameColumn(
                name: "SizeBytes",
                table: "edpf_authaud_arcjob",
                newName: "size_byt");

            migrationBuilder.RenameColumn(
                name: "RecordCount",
                table: "edpf_authaud_arcjob",
                newName: "rec_cnt");

            migrationBuilder.RenameColumn(
                name: "FromUtc",
                table: "edpf_authaud_arcjob",
                newName: "from_utc");

            migrationBuilder.RenameColumn(
                name: "FailureReason",
                table: "edpf_authaud_arcjob",
                newName: "fail_rsn");

            migrationBuilder.RenameColumn(
                name: "ArchiveId",
                table: "edpf_authaud_arcjob",
                newName: "arcid");

            migrationBuilder.RenameColumn(
                name: "authaudarcjobid",
                table: "edpf_authaud_arcjob",
                newName: "auth_aud_arc_jobid");

            migrationBuilder.RenameColumn(
                name: "fnsh_utc",
                table: "edpf_authaud_arcattm",
                newName: "fin_utc");

            migrationBuilder.RenameColumn(
                name: "ResultCode",
                table: "edpf_authaud_arcattm",
                newName: "result_code");

            migrationBuilder.RenameColumn(
                name: "IsSuccessful",
                table: "edpf_authaud_arcattm",
                newName: "succ_yn");

            migrationBuilder.RenameColumn(
                name: "AttemptNumber",
                table: "edpf_authaud_arcattm",
                newName: "attm_num");

            migrationBuilder.RenameColumn(
                name: "authaudarcattmid",
                table: "edpf_authaud_arcattm",
                newName: "auth_aud_arc_attmid");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "edpf_authaud",
                newName: "scp");

            migrationBuilder.RenameColumn(
                name: "ReasonCode",
                table: "edpf_authaud",
                newName: "rsn_cd");

            migrationBuilder.RenameColumn(
                name: "MatchedPolicyId",
                table: "edpf_authaud",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "IsAllowed",
                table: "edpf_authaud",
                newName: "allow_yn");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "edpf_authaud",
                newName: "act");

            migrationBuilder.RenameColumn(
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_str_qta_plcyid_tntid",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid",
                table: "edpf_authaud_arcjob");

            migrationBuilder.RenameColumn(
                name: "tnt_rtn_alwid",
                table: "edpf_tntrtnalw",
                newName: "tntrtnalwid");

            migrationBuilder.RenameColumn(
                name: "tnt_mut_audid",
                table: "edpf_tntmut_aud",
                newName: "tntmutaudid");

            migrationBuilder.RenameColumn(
                name: "tnt_mbrid",
                table: "edpf_tntmbr",
                newName: "tntmbrid");

            migrationBuilder.RenameColumn(
                name: "tnt_dbndid",
                table: "edpf_tntdbnd",
                newName: "tntdbndid");

            migrationBuilder.RenameColumn(
                name: "tntnm",
                table: "edpf_tnt",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "tntcd",
                table: "edpf_tnt",
                newName: "cd");

            migrationBuilder.RenameColumn(
                name: "occ_utc",
                table: "edpf_strqta_plcychg",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcy_chgid",
                table: "edpf_strqta_plcychg",
                newName: "strqtaplcychgid");

            migrationBuilder.RenameColumn(
                name: "warn_thr_pct",
                table: "edpf_strqta_plcy",
                newName: "WarningThresholdPercent");

            migrationBuilder.RenameColumn(
                name: "upd_subid",
                table: "edpf_strqta_plcy",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "tnt_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "TenantLimitBytes");

            migrationBuilder.RenameColumn(
                name: "tgtid",
                table: "edpf_strqta_plcy",
                newName: "TargetId");

            migrationBuilder.RenameColumn(
                name: "scp",
                table: "edpf_strqta_plcy",
                newName: "Scope");

            migrationBuilder.RenameColumn(
                name: "prj_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "ProjectLimitBytes");

            migrationBuilder.RenameColumn(
                name: "plan_cd",
                table: "edpf_strqta_plcy",
                newName: "PlanCode");

            migrationBuilder.RenameColumn(
                name: "org_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "OrganizationLimitBytes");

            migrationBuilder.RenameColumn(
                name: "dset_lmt_byt",
                table: "edpf_strqta_plcy",
                newName: "DataSetLimitBytes");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcyid",
                table: "edpf_strqta_plcy",
                newName: "strqtaplcyid");

            migrationBuilder.RenameColumn(
                name: "rsrcnm",
                table: "edpf_rsrc",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "prj_mbrid",
                table: "edpf_prjmbr",
                newName: "prjmbrid");

            migrationBuilder.RenameColumn(
                name: "prjnm",
                table: "edpf_prj",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "prjcd",
                table: "edpf_prj",
                newName: "cd");

            migrationBuilder.RenameColumn(
                name: "val",
                table: "edpf_plcy",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "plcykey",
                table: "edpf_plcy",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "org_mbrid",
                table: "edpf_orgmbr",
                newName: "orgmbrid");

            migrationBuilder.RenameColumn(
                name: "orgnm",
                table: "edpf_org",
                newName: "nm");

            migrationBuilder.RenameColumn(
                name: "orgcd",
                table: "edpf_org",
                newName: "cd");

            migrationBuilder.RenameColumn(
                name: "occ_utc",
                table: "edpf_authplcy_chg",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "auth_plcy_chgid",
                table: "edpf_authplcy_chg",
                newName: "authplcychgid");

            migrationBuilder.RenameColumn(
                name: "pri",
                table: "edpf_authplcy",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "eff",
                table: "edpf_authplcy",
                newName: "Effect");

            migrationBuilder.RenameColumn(
                name: "act",
                table: "edpf_authplcy",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "auth_plcyid",
                table: "edpf_authplcy",
                newName: "authplcyid");

            migrationBuilder.RenameColumn(
                name: "occ_utc",
                table: "edpf_authaud_rtnchg",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtn_chgid",
                table: "edpf_authaud_rtnchg",
                newName: "authaudrtnchgid");

            migrationBuilder.RenameColumn(
                name: "upd_subid",
                table: "edpf_authaud_rtn",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "rtn_day",
                table: "edpf_authaud_rtn",
                newName: "rtn_days");

            migrationBuilder.RenameColumn(
                name: "legal_hold_yn",
                table: "edpf_authaud_rtn",
                newName: "IsLegalHold");

            migrationBuilder.RenameColumn(
                name: "legal_hold_rsn",
                table: "edpf_authaud_rtn",
                newName: "LegalHoldReason");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtnid",
                table: "edpf_authaud_rtn",
                newName: "authaudrtnid");

            migrationBuilder.RenameColumn(
                name: "leaseid",
                table: "edpf_authaud_arcschd",
                newName: "LeaseId");

            migrationBuilder.RenameColumn(
                name: "nxt_utc",
                table: "edpf_authaud_arcschd",
                newName: "nxt_run_utc");

            migrationBuilder.RenameColumn(
                name: "last_utc",
                table: "edpf_authaud_arcschd",
                newName: "last_run_utc");

            migrationBuilder.RenameColumn(
                name: "int_hr",
                table: "edpf_authaud_arcschd",
                newName: "IntervalHours");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_schdid",
                table: "edpf_authaud_arcschd",
                newName: "authaudarcschdid");

            migrationBuilder.RenameColumn(
                name: "sha256",
                table: "edpf_authaud_arcjob",
                newName: "Sha256");

            migrationBuilder.RenameColumn(
                name: "to_utc",
                table: "edpf_authaud_arcjob",
                newName: "ToUtc");

            migrationBuilder.RenameColumn(
                name: "size_byt",
                table: "edpf_authaud_arcjob",
                newName: "SizeBytes");

            migrationBuilder.RenameColumn(
                name: "rec_cnt",
                table: "edpf_authaud_arcjob",
                newName: "RecordCount");

            migrationBuilder.RenameColumn(
                name: "from_utc",
                table: "edpf_authaud_arcjob",
                newName: "FromUtc");

            migrationBuilder.RenameColumn(
                name: "fail_rsn",
                table: "edpf_authaud_arcjob",
                newName: "FailureReason");

            migrationBuilder.RenameColumn(
                name: "arcid",
                table: "edpf_authaud_arcjob",
                newName: "ArchiveId");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_jobid",
                table: "edpf_authaud_arcjob",
                newName: "authaudarcjobid");

            migrationBuilder.RenameColumn(
                name: "succ_yn",
                table: "edpf_authaud_arcattm",
                newName: "IsSuccessful");

            migrationBuilder.RenameColumn(
                name: "result_code",
                table: "edpf_authaud_arcattm",
                newName: "ResultCode");

            migrationBuilder.RenameColumn(
                name: "fin_utc",
                table: "edpf_authaud_arcattm",
                newName: "fnsh_utc");

            migrationBuilder.RenameColumn(
                name: "attm_num",
                table: "edpf_authaud_arcattm",
                newName: "AttemptNumber");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_attmid",
                table: "edpf_authaud_arcattm",
                newName: "authaudarcattmid");

            migrationBuilder.RenameColumn(
                name: "scp",
                table: "edpf_authaud",
                newName: "Scope");

            migrationBuilder.RenameColumn(
                name: "rsn_cd",
                table: "edpf_authaud",
                newName: "ReasonCode");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_authaud",
                newName: "MatchedPolicyId");

            migrationBuilder.RenameColumn(
                name: "occ_utc",
                table: "edpf_authaud",
                newName: "occr_utc");

            migrationBuilder.RenameColumn(
                name: "allow_yn",
                table: "edpf_authaud",
                newName: "IsAllowed");

            migrationBuilder.RenameColumn(
                name: "act",
                table: "edpf_authaud",
                newName: "Action");

            migrationBuilder.RenameColumn(
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileApprovedDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "tntrtnalwid", table: "edpf_tntrtnalw", newName: "tnt_rtn_alwid");
            migrationBuilder.RenameColumn(name: "strqtaplcychgid", table: "edpf_strqta_plcychg", newName: "str_qta_plcy_chgid");
            migrationBuilder.RenameColumn(name: "strqtaplcyid", table: "edpf_strqta_plcy", newName: "str_qta_plcyid");
            migrationBuilder.RenameColumn(name: "authaudrtnchgid", table: "edpf_authaud_rtnchg", newName: "auth_aud_rtn_chgid");
            migrationBuilder.RenameColumn(name: "authaudrtnid", table: "edpf_authaud_rtn", newName: "auth_aud_rtnid");
            migrationBuilder.RenameColumn(name: "authaudarcjobid", table: "edpf_authaud_arcjob", newName: "auth_aud_arc_jobid");
            migrationBuilder.RenameColumn(name: "ArchiveId", table: "edpf_authaud_arcjob", newName: "arcid");
            migrationBuilder.RenameColumn(name: "FailureReason", table: "edpf_authaud_arcjob", newName: "fail_rsn");
            migrationBuilder.RenameColumn(name: "FromUtc", table: "edpf_authaud_arcjob", newName: "from_utc");
            migrationBuilder.RenameColumn(name: "ToUtc", table: "edpf_authaud_arcjob", newName: "to_utc");
            migrationBuilder.RenameColumn(name: "RecordCount", table: "edpf_authaud_arcjob", newName: "rec_cnt");
            migrationBuilder.RenameColumn(name: "SizeBytes", table: "edpf_authaud_arcjob", newName: "size_byt");
            migrationBuilder.RenameColumn(name: "authaudarcattmid", table: "edpf_authaud_arcattm", newName: "auth_aud_arc_attmid");
            migrationBuilder.RenameColumn(name: "AttemptNumber", table: "edpf_authaud_arcattm", newName: "attm_num");
            migrationBuilder.RenameColumn(name: "fnsh_utc", table: "edpf_authaud_arcattm", newName: "fin_utc");
            migrationBuilder.RenameColumn(name: "IsSuccessful", table: "edpf_authaud_arcattm", newName: "succ_yn");
            migrationBuilder.RenameColumn(name: "ResultCode", table: "edpf_authaud_arcattm", newName: "result_code");
            migrationBuilder.RenameColumn(name: "TenantLimitBytes", table: "edpf_strqta_plcy", newName: "tnt_lmt_byt");
            migrationBuilder.RenameColumn(name: "OrganizationLimitBytes", table: "edpf_strqta_plcy", newName: "org_lmt_byt");
            migrationBuilder.RenameColumn(name: "ProjectLimitBytes", table: "edpf_strqta_plcy", newName: "prj_lmt_byt");
            migrationBuilder.RenameColumn(name: "DataSetLimitBytes", table: "edpf_strqta_plcy", newName: "dset_lmt_byt");
            migrationBuilder.RenameColumn(name: "WarningThresholdPercent", table: "edpf_strqta_plcy", newName: "warn_thr_pct");
            migrationBuilder.RenameColumn(name: "PlanCode", table: "edpf_strqta_plcy", newName: "plan_cd");
            migrationBuilder.RenameColumn(name: "UpdatedBy", table: "edpf_strqta_plcy", newName: "upd_subid");
            migrationBuilder.RenameColumn(name: "Scope", table: "edpf_strqta_plcy", newName: "scp");
            migrationBuilder.RenameColumn(name: "TargetId", table: "edpf_strqta_plcy", newName: "tgtid");
            migrationBuilder.RenameColumn(name: "IsLegalHold", table: "edpf_authaud_rtn", newName: "legal_hold_yn");
            migrationBuilder.RenameColumn(name: "LegalHoldReason", table: "edpf_authaud_rtn", newName: "legal_hold_rsn");
            migrationBuilder.RenameColumn(name: "rtn_days", table: "edpf_authaud_rtn", newName: "rtn_day");
            migrationBuilder.RenameColumn(name: "UpdatedBy", table: "edpf_authaud_rtn", newName: "upd_subid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "tnt_rtn_alwid", table: "edpf_tntrtnalw", newName: "tntrtnalwid");
            migrationBuilder.RenameColumn(name: "str_qta_plcy_chgid", table: "edpf_strqta_plcychg", newName: "strqtaplcychgid");
            migrationBuilder.RenameColumn(name: "str_qta_plcyid", table: "edpf_strqta_plcy", newName: "strqtaplcyid");
            migrationBuilder.RenameColumn(name: "auth_aud_rtn_chgid", table: "edpf_authaud_rtnchg", newName: "authaudrtnchgid");
            migrationBuilder.RenameColumn(name: "auth_aud_rtnid", table: "edpf_authaud_rtn", newName: "authaudrtnid");
            migrationBuilder.RenameColumn(name: "auth_aud_arc_jobid", table: "edpf_authaud_arcjob", newName: "authaudarcjobid");
            migrationBuilder.RenameColumn(name: "arcid", table: "edpf_authaud_arcjob", newName: "ArchiveId");
            migrationBuilder.RenameColumn(name: "fail_rsn", table: "edpf_authaud_arcjob", newName: "FailureReason");
            migrationBuilder.RenameColumn(name: "from_utc", table: "edpf_authaud_arcjob", newName: "FromUtc");
            migrationBuilder.RenameColumn(name: "to_utc", table: "edpf_authaud_arcjob", newName: "ToUtc");
            migrationBuilder.RenameColumn(name: "rec_cnt", table: "edpf_authaud_arcjob", newName: "RecordCount");
            migrationBuilder.RenameColumn(name: "size_byt", table: "edpf_authaud_arcjob", newName: "SizeBytes");
            migrationBuilder.RenameColumn(name: "auth_aud_arc_attmid", table: "edpf_authaud_arcattm", newName: "authaudarcattmid");
            migrationBuilder.RenameColumn(name: "attm_num", table: "edpf_authaud_arcattm", newName: "AttemptNumber");
            migrationBuilder.RenameColumn(name: "fin_utc", table: "edpf_authaud_arcattm", newName: "fnsh_utc");
            migrationBuilder.RenameColumn(name: "succ_yn", table: "edpf_authaud_arcattm", newName: "IsSuccessful");
            migrationBuilder.RenameColumn(name: "result_code", table: "edpf_authaud_arcattm", newName: "ResultCode");
            migrationBuilder.RenameColumn(name: "tnt_lmt_byt", table: "edpf_strqta_plcy", newName: "TenantLimitBytes");
            migrationBuilder.RenameColumn(name: "org_lmt_byt", table: "edpf_strqta_plcy", newName: "OrganizationLimitBytes");
            migrationBuilder.RenameColumn(name: "prj_lmt_byt", table: "edpf_strqta_plcy", newName: "ProjectLimitBytes");
            migrationBuilder.RenameColumn(name: "dset_lmt_byt", table: "edpf_strqta_plcy", newName: "DataSetLimitBytes");
            migrationBuilder.RenameColumn(name: "warn_thr_pct", table: "edpf_strqta_plcy", newName: "WarningThresholdPercent");
            migrationBuilder.RenameColumn(name: "plan_cd", table: "edpf_strqta_plcy", newName: "PlanCode");
            migrationBuilder.RenameColumn(name: "upd_subid", table: "edpf_strqta_plcy", newName: "UpdatedBy");
            migrationBuilder.RenameColumn(name: "scp", table: "edpf_strqta_plcy", newName: "Scope");
            migrationBuilder.RenameColumn(name: "tgtid", table: "edpf_strqta_plcy", newName: "TargetId");
            migrationBuilder.RenameColumn(name: "legal_hold_yn", table: "edpf_authaud_rtn", newName: "IsLegalHold");
            migrationBuilder.RenameColumn(name: "legal_hold_rsn", table: "edpf_authaud_rtn", newName: "LegalHoldReason");
            migrationBuilder.RenameColumn(name: "rtn_day", table: "edpf_authaud_rtn", newName: "rtn_days");
            migrationBuilder.RenameColumn(name: "upd_subid", table: "edpf_authaud_rtn", newName: "UpdatedBy");
        }
    }
}

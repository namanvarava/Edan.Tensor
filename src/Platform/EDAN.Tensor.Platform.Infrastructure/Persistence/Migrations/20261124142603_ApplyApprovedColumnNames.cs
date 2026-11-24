using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "edpf_plcy",
                newName: "plcykey");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "edpf_plcy",
                newName: "val");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_usr",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_usr",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "edpf_usr",
                newName: "eml");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "edpf_usr",
                newName: "dspnm");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_usr",
                newName: "usrid");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "edpf_tntrtnalw",
                newName: "upd_utc");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_tntrtnalw",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SchemaName",
                table: "edpf_tntrtnalw",
                newName: "schm_nm");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edpf_tntrtnalw",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_tntrtnalw",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_tntrtnalw",
                newName: "tntrtnalwid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_tntmut_aud",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_tntmut_aud",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "edpf_tntmut_aud",
                newName: "idem_key");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_tntmut_aud",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "edpf_tntmut_aud",
                newName: "cmp_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_tntmut_aud",
                newName: "tnt_mut_audid");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "edpf_tntmbr",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_tntmbr",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "edpf_tntmbr",
                newName: "subid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_tntmbr",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_tntmbr",
                newName: "tnt_mbrid");

            migrationBuilder.RenameColumn(
                name: "Port",
                table: "edpf_tntdbnd",
                newName: "port");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "edpf_tntdbnd",
                newName: "usrnm");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "edpf_tntdbnd",
                newName: "upd_utc");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_tntdbnd",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_tntdbnd",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ServerAddress",
                table: "edpf_tntdbnd",
                newName: "svr_addr");

            migrationBuilder.RenameColumn(
                name: "SecretKey",
                table: "edpf_tntdbnd",
                newName: "scrt_key");

            migrationBuilder.RenameColumn(
                name: "SchemaName",
                table: "edpf_tntdbnd",
                newName: "schm_nm");

            migrationBuilder.RenameColumn(
                name: "Provider",
                table: "edpf_tntdbnd",
                newName: "prvdr");

            migrationBuilder.RenameColumn(
                name: "DatabaseName",
                table: "edpf_tntdbnd",
                newName: "db_nm");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_tntdbnd",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_tntdbnd",
                newName: "tnt_dbndid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_tnt",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "edpf_tnt",
                newName: "tntnm");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_tnt",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "edpf_tnt",
                newName: "tntcd");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_tnt",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_strqta_plcychg",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "edpf_strqta_plcychg",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "edpf_strqta_plcychg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "ChangeType",
                table: "edpf_strqta_plcychg",
                newName: "chg_typ");

            migrationBuilder.RenameColumn(
                name: "BeforeJson",
                table: "edpf_strqta_plcychg",
                newName: "bef_json");

            migrationBuilder.RenameColumn(
                name: "AfterJson",
                table: "edpf_strqta_plcychg",
                newName: "aft_json");

            migrationBuilder.RenameColumn(
                name: "ActorSubjectId",
                table: "edpf_strqta_plcychg",
                newName: "act_subid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_strqta_plcychg",
                newName: "strqtaplcychgid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edpf_strqta_plcy",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "edpf_strqta_plcy",
                newName: "upd_utc");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_strqta_plcy",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edpf_strqta_plcy",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_strqta_plcy",
                newName: "strqtaplcyid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_rsrc",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ResourceType",
                table: "edpf_rsrc",
                newName: "rsrc_typ");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "edpf_rsrc",
                newName: "rsrcnm");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_rsrc",
                newName: "rsrcid");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "edpf_prjmbr",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_prjmbr",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "edpf_prjmbr",
                newName: "subid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_prjmbr",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "edpf_prjmbr",
                newName: "prjid");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "edpf_prjmbr",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_prjmbr",
                newName: "prj_mbrid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_prj",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_prj",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "edpf_prj",
                newName: "orgid");

            migrationBuilder.Sql("ALTER TABLE \"edpf_prj\" RENAME COLUMN \"Name\" TO \"prjnm\";");
            migrationBuilder.Sql("ALTER TABLE \"edpf_prj\" RENAME COLUMN \"Code\" TO \"prjcd\";");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_prj",
                newName: "prjid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edpf_plcy",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edpf_plcy",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_plcy",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "edpf_orgmbr",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_orgmbr",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "edpf_orgmbr",
                newName: "subid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_orgmbr",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "edpf_orgmbr",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_orgmbr",
                newName: "org_mbrid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_org",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_org",
                newName: "st");

            migrationBuilder.Sql("ALTER TABLE \"edpf_org\" RENAME COLUMN \"Name\" TO \"orgnm\";");
            migrationBuilder.Sql("ALTER TABLE \"edpf_org\" RENAME COLUMN \"Code\" TO \"orgcd\";");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_org",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "edpf_inv",
                newName: "usrid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_inv",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_inv",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "edpf_inv",
                newName: "eml");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "edpf_inv",
                newName: "dspnm");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_inv",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_inv",
                newName: "invid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authplcy_chg",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "edpf_authplcy_chg",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "ChangeType",
                table: "edpf_authplcy_chg",
                newName: "chg_typ");

            migrationBuilder.RenameColumn(
                name: "BeforeJson",
                table: "edpf_authplcy_chg",
                newName: "bef_json");

            migrationBuilder.RenameColumn(
                name: "AfterJson",
                table: "edpf_authplcy_chg",
                newName: "aft_json");

            migrationBuilder.RenameColumn(
                name: "ActorSubjectId",
                table: "edpf_authplcy_chg",
                newName: "act_subid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authplcy_chg",
                newName: "auth_plcy_chgid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authplcy",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "edpf_authplcy",
                newName: "subid");

            migrationBuilder.RenameColumn(
                name: "ResourceType",
                table: "edpf_authplcy",
                newName: "rsrc_typ");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "edpf_authplcy",
                newName: "prjid");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "edpf_authplcy",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edpf_authplcy",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_authplcy",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "edpf_authplcy",
                newName: "act");

            migrationBuilder.RenameColumn(
                name: "Effect",
                table: "edpf_authplcy",
                newName: "eff");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "edpf_authplcy",
                newName: "pri");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authplcy",
                newName: "auth_plcyid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud_rtnchg",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "edpf_authaud_rtnchg",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "edpf_authaud_rtnchg",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "ChangeType",
                table: "edpf_authaud_rtnchg",
                newName: "chg_typ");

            migrationBuilder.RenameColumn(
                name: "BeforeJson",
                table: "edpf_authaud_rtnchg",
                newName: "bef_json");

            migrationBuilder.RenameColumn(
                name: "AfterJson",
                table: "edpf_authaud_rtnchg",
                newName: "aft_json");

            migrationBuilder.RenameColumn(
                name: "ActorSubjectId",
                table: "edpf_authaud_rtnchg",
                newName: "act_subid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud_rtnchg",
                newName: "authaudrtnchgid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edpf_authaud_rtn",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "edpf_authaud_rtn",
                newName: "upd_utc");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud_rtn",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "RetentionDays",
                table: "edpf_authaud_rtn",
                newName: "rtn_days");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud_rtn",
                newName: "authaudrtnid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edpf_authaud_arcschd",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud_arcschd",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "NextRunAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "nxt_utc");

            migrationBuilder.RenameColumn(
                name: "LeaseExpiresAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "lease_exp_utc");

            migrationBuilder.RenameColumn(
                name: "LastRunAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "last_utc");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edpf_authaud_arcschd",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "IntervalHours",
                table: "edpf_authaud_arcschd",
                newName: "int_hr");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud_arcschd",
                newName: "auth_aud_arc_schdid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edpf_authaud_arcjob",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud_arcjob",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edpf_authaud_arcjob",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edpf_authaud_arcjob",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "edpf_authaud_arcjob",
                newName: "cmp_utc");

            migrationBuilder.RenameColumn(
                name: "AttemptCount",
                table: "edpf_authaud_arcjob",
                newName: "attm_cnt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud_arcjob",
                newName: "authaudarcjobid");

            migrationBuilder.RenameColumn(
                name: "JobId",
                table: "edpf_authaud_arcattm",
                newName: "jobid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud_arcattm",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "StartedAtUtc",
                table: "edpf_authaud_arcattm",
                newName: "strt_utc");

            migrationBuilder.RenameColumn(
                name: "FinishedAtUtc",
                table: "edpf_authaud_arcattm",
                newName: "fnsh_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud_arcattm",
                newName: "authaudarcattmid");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edpf_authaud",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "edpf_authaud",
                newName: "subid");

            migrationBuilder.RenameColumn(
                name: "ResourceType",
                table: "edpf_authaud",
                newName: "rsrc_typ");

            migrationBuilder.RenameColumn(
                name: "OccurredAtUtc",
                table: "edpf_authaud",
                newName: "occ_utc");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "edpf_authaud",
                newName: "act");

            migrationBuilder.RenameColumn(
                name: "IsAllowed",
                table: "edpf_authaud",
                newName: "allow_yn");

            migrationBuilder.RenameColumn(
                name: "MatchedPolicyId",
                table: "edpf_authaud",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "ReasonCode",
                table: "edpf_authaud",
                newName: "rsn_cd");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "edpf_authaud",
                newName: "scp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edpf_authaud",
                newName: "auth_audid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_strqtaplcyid_tntid",
                table: "edpf_strqta_plcy",
                columns: new[] { "strqtaplcyid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_prjid_orgid_tntid",
                table: "edpf_prj",
                columns: new[] { "prjid", "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_orgid_tntid",
                table: "edpf_org",
                columns: new[] { "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_invid_tntid",
                table: "edpf_inv",
                columns: new[] { "invid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_authaudarcjobid_tntid",
                table: "edpf_authaud_arcjob",
                columns: new[] { "authaudarcjobid", "tntid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_strqtaplcyid_tntid",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_prj_prjid_orgid_tntid",
                table: "edpf_prj");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_org_orgid_tntid",
                table: "edpf_org");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_inv_invid_tntid",
                table: "edpf_inv");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_authaudarcjobid_tntid",
                table: "edpf_authaud_arcjob");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_usr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_usr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "eml",
                table: "edpf_usr",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "dspnm",
                table: "edpf_usr",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edpf_usr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_tntrtnalw",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntrtnalw",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "schm_nm",
                table: "edpf_tntrtnalw",
                newName: "SchemaName");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_tntrtnalw",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntrtnalw",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntrtnalwid",
                table: "edpf_tntrtnalw",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntmut_aud",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntmut_aud",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "edpf_tntmut_aud",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntmut_aud",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edpf_tntmut_aud",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tnt_mut_audid",
                table: "edpf_tntmut_aud",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_tntmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_tntmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "tnt_mbrid",
                table: "edpf_tntmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "port",
                table: "edpf_tntdbnd",
                newName: "Port");

            migrationBuilder.RenameColumn(
                name: "usrnm",
                table: "edpf_tntdbnd",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_tntdbnd",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntdbnd",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "svr_addr",
                table: "edpf_tntdbnd",
                newName: "ServerAddress");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntdbnd",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scrt_key",
                table: "edpf_tntdbnd",
                newName: "SecretKey");

            migrationBuilder.RenameColumn(
                name: "schm_nm",
                table: "edpf_tntdbnd",
                newName: "SchemaName");

            migrationBuilder.RenameColumn(
                name: "prvdr",
                table: "edpf_tntdbnd",
                newName: "Provider");

            migrationBuilder.RenameColumn(
                name: "db_nm",
                table: "edpf_tntdbnd",
                newName: "DatabaseName");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntdbnd",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tnt_dbndid",
                table: "edpf_tntdbnd",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tnt",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "tntnm",
                table: "edpf_tnt",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tnt",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntcd",
                table: "edpf_tnt",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tnt",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_strqta_plcychg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_strqta_plcychg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_strqta_plcychg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_strqta_plcychg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_strqta_plcychg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_strqta_plcychg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_strqta_plcychg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "strqtaplcychgid",
                table: "edpf_strqta_plcychg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_strqta_plcy",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_strqta_plcy",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_strqta_plcy",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_strqta_plcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "strqtaplcyid",
                table: "edpf_strqta_plcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_rsrc",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_rsrc",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_rsrc",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "rsrcid",
                table: "edpf_rsrc",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_prjmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_prjmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_prjmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_prjmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_prjmbr",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_prjmbr",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "prjmbrid",
                table: "edpf_prjmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_prj",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_prj",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_prj",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_prj",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_prj",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_prj",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_plcy",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_plcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_plcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_orgmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_orgmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_orgmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_orgmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_orgmbr",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "orgmbrid",
                table: "edpf_orgmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_org",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_org",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_org",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_org",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_org",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edpf_inv",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_inv",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_inv",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "eml",
                table: "edpf_inv",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "dspnm",
                table: "edpf_inv",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_inv",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "invid",
                table: "edpf_inv",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authplcy_chg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_authplcy_chg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authplcy_chg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_authplcy_chg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_authplcy_chg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_authplcy_chg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_authplcy_chg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "authplcychgid",
                table: "edpf_authplcy_chg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authplcy",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_authplcy",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_authplcy",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_authplcy",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_authplcy",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_authplcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_authplcy",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "auth_plcyid",
                table: "edpf_authplcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_rtnchg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_authaud_rtnchg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud_rtnchg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_authaud_rtnchg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_authaud_rtnchg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_authaud_rtnchg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_authaud_rtnchg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "authaudrtnchgid",
                table: "edpf_authaud_rtnchg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_rtn",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_authaud_rtn",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_rtn",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "rtn_days",
                table: "edpf_authaud_rtn",
                newName: "RetentionDays");

            migrationBuilder.RenameColumn(
                name: "authaudrtnid",
                table: "edpf_authaud_rtn",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_arcschd",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcschd",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "nxt_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "NextRunAtUtc");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc",
                table: "edpf_authaud_arcschd",
                newName: "LeaseExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "LastRunAtUtc");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_authaud_arcschd",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "authaudarcschdid",
                table: "edpf_authaud_arcschd",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_arcjob",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcjob",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_authaud_arcjob",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_authaud_arcjob",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edpf_authaud_arcjob",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "edpf_authaud_arcjob",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "authaudarcjobid",
                table: "edpf_authaud_arcjob",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "jobid",
                table: "edpf_authaud_arcattm",
                newName: "JobId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcattm",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "strt_utc",
                table: "edpf_authaud_arcattm",
                newName: "StartedAtUtc");

            migrationBuilder.RenameColumn(
                name: "fnsh_utc",
                table: "edpf_authaud_arcattm",
                newName: "FinishedAtUtc");

            migrationBuilder.RenameColumn(
                name: "authaudarcattmid",
                table: "edpf_authaud_arcattm",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_authaud",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_authaud",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "act",
                table: "edpf_authaud",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "auth_audid",
                table: "edpf_authaud",
                newName: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "Id", "OrganizationId", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob",
                columns: new[] { "Id", "TenantId" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>기존 SQLite 테이블을 승인된 전자사전 테이블명으로 변경합니다.</summary>
public partial class ApplyApprovedTableNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => RenameTables(migrationBuilder, false);

    protected override void Down(MigrationBuilder migrationBuilder) => RenameTables(migrationBuilder, true);

    private static void RenameTables(MigrationBuilder migrationBuilder, bool reverse)
    {
        var mappings = new (string OldName, string NewName)[]
        {
            ("platform_users", "edpf_usr"), ("platform_tenants", "edpf_tnt"),
            ("platform_tenant_memberships", "edpf_tntmbr"),
            ("platform_tenant_database_routine_allowlists", "edpf_tntrtnalw"),
            ("platform_tenant_database_mutation_audits", "edpf_tntmut_aud"),
            ("platform_tenant_database_bindings", "edpf_tntdbnd"),
            ("platform_storage_quota_policy_changes", "edpf_strqta_plcychg"),
            ("platform_storage_quota_policies", "edpf_strqta_plcy"),
            ("platform_resources", "edpf_rsrc"), ("platform_projects", "edpf_prj"),
            ("platform_project_memberships", "edpf_prjmbr"), ("platform_policies", "edpf_plcy"),
            ("platform_organizations", "edpf_org"), ("platform_organization_memberships", "edpf_orgmbr"),
            ("platform_invitations", "edpf_inv"),
            ("platform_authorization_policy_changes", "edpf_authplcy_chg"),
            ("platform_authorization_policies", "edpf_authplcy"),
            ("platform_authorization_audit_retention_changes", "edpf_authaud_rtnchg"),
            ("platform_authorization_audit_retention", "edpf_authaud_rtn"),
            ("platform_authorization_audit_archive_schedules", "edpf_authaud_arcschd"),
            ("platform_authorization_audit_archive_jobs", "edpf_authaud_arcjob"),
            ("platform_authorization_audit_archive_attempts", "edpf_authaud_arcattm"),
            ("platform_authorization_audit", "edpf_authaud")
        };

        foreach (var (oldName, newName) in mappings)
        {
            var from = reverse ? newName : oldName;
            var to = reverse ? oldName : newName;
            migrationBuilder.Sql($"ALTER TABLE \"{from}\" RENAME TO \"{to}\";");
        }
    }
}

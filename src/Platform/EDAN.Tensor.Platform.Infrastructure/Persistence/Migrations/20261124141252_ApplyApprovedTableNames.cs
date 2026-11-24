using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_platform_tenants_TenantId",
                table: "platform_authorization_audit");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_archive_attempts_platform_authorization_audit_archive_jobs_JobId_TenantId",
                table: "platform_authorization_audit_archive_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_archive_attempts_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_archive_jobs_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_archive_schedules_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_retention_platform_tenants_TenantId",
                table: "platform_authorization_audit_retention");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_audit_retention_changes_platform_tenants_TenantId",
                table: "platform_authorization_audit_retention_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_policies_platform_organizations_OrganizationId_TenantId",
                table: "platform_authorization_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_policies_platform_projects_ProjectId_OrganizationId_TenantId",
                table: "platform_authorization_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_policies_platform_tenants_TenantId",
                table: "platform_authorization_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_authorization_policy_changes_platform_tenants_TenantId",
                table: "platform_authorization_policy_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_invitations_platform_tenants_TenantId",
                table: "platform_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_invitations_platform_users_UserId",
                table: "platform_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_organization_memberships_platform_organizations_OrganizationId_TenantId",
                table: "platform_organization_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_organizations_platform_tenants_TenantId",
                table: "platform_organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_policies_platform_organizations_organization_id_tenant_id",
                table: "platform_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_policies_platform_projects_project_id_organization_id_tenant_id",
                table: "platform_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_policies_platform_tenants_tenant_id",
                table: "platform_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_project_memberships_platform_projects_ProjectId_OrganizationId_TenantId",
                table: "platform_project_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_projects_platform_organizations_OrganizationId_TenantId",
                table: "platform_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_projects_platform_tenants_TenantId",
                table: "platform_projects");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_resources_platform_organizations_organization_id_tenant_id",
                table: "platform_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_resources_platform_projects_project_id_organization_id_tenant_id",
                table: "platform_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_resources_platform_tenants_tenant_id",
                table: "platform_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_storage_quota_policies_platform_tenants_TenantId",
                table: "platform_storage_quota_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_storage_quota_policy_changes_platform_storage_quota_policies_PolicyId_TenantId",
                table: "platform_storage_quota_policy_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_storage_quota_policy_changes_platform_tenants_TenantId",
                table: "platform_storage_quota_policy_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_database_bindings_platform_tenants_TenantId",
                table: "platform_tenant_database_bindings");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId",
                table: "platform_tenant_database_mutation_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId",
                table: "platform_tenant_database_routine_allowlists");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_memberships_platform_tenants_TenantId",
                table: "platform_tenant_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_users_platform_tenants_TenantId",
                table: "platform_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_users",
                table: "platform_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenants",
                table: "platform_tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenant_memberships",
                table: "platform_tenant_memberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenant_database_routine_allowlists",
                table: "platform_tenant_database_routine_allowlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenant_database_mutation_audits",
                table: "platform_tenant_database_mutation_audits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenant_database_bindings",
                table: "platform_tenant_database_bindings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_storage_quota_policy_changes",
                table: "platform_storage_quota_policy_changes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_platform_storage_quota_policies_Id_TenantId",
                table: "platform_storage_quota_policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_storage_quota_policies",
                table: "platform_storage_quota_policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_resources",
                table: "platform_resources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_platform_resources_project_requires_organization",
                table: "platform_resources");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_platform_projects_Id_OrganizationId_TenantId",
                table: "platform_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_projects",
                table: "platform_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_project_memberships",
                table: "platform_project_memberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_policies",
                table: "platform_policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_platform_policies_project_requires_organization",
                table: "platform_policies");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_platform_organizations_Id_TenantId",
                table: "platform_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_organizations",
                table: "platform_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_organization_memberships",
                table: "platform_organization_memberships");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_platform_invitations_Id_TenantId",
                table: "platform_invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_invitations",
                table: "platform_invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_policy_changes",
                table: "platform_authorization_policy_changes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_policies",
                table: "platform_authorization_policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit_retention_changes",
                table: "platform_authorization_audit_retention_changes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit_retention",
                table: "platform_authorization_audit_retention");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit_archive_schedules",
                table: "platform_authorization_audit_archive_schedules");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_platform_authorization_audit_archive_jobs_Id_TenantId",
                table: "platform_authorization_audit_archive_jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit_archive_jobs",
                table: "platform_authorization_audit_archive_jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit_archive_attempts",
                table: "platform_authorization_audit_archive_attempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_authorization_audit",
                table: "platform_authorization_audit");

            migrationBuilder.RenameTable(
                name: "platform_users",
                newName: "edpf_usr");

            migrationBuilder.RenameTable(
                name: "platform_tenants",
                newName: "edpf_tnt");

            migrationBuilder.RenameTable(
                name: "platform_tenant_memberships",
                newName: "edpf_tntmbr");

            migrationBuilder.RenameTable(
                name: "platform_tenant_database_routine_allowlists",
                newName: "edpf_tntrtnalw");

            migrationBuilder.RenameTable(
                name: "platform_tenant_database_mutation_audits",
                newName: "edpf_tntmut_aud");

            migrationBuilder.RenameTable(
                name: "platform_tenant_database_bindings",
                newName: "edpf_tntdbnd");

            migrationBuilder.RenameTable(
                name: "platform_storage_quota_policy_changes",
                newName: "edpf_strqta_plcychg");

            migrationBuilder.RenameTable(
                name: "platform_storage_quota_policies",
                newName: "edpf_strqta_plcy");

            migrationBuilder.RenameTable(
                name: "platform_resources",
                newName: "edpf_rsrc");

            migrationBuilder.RenameTable(
                name: "platform_projects",
                newName: "edpf_prj");

            migrationBuilder.RenameTable(
                name: "platform_project_memberships",
                newName: "edpf_prjmbr");

            migrationBuilder.RenameTable(
                name: "platform_policies",
                newName: "edpf_plcy");

            migrationBuilder.RenameTable(
                name: "platform_organizations",
                newName: "edpf_org");

            migrationBuilder.RenameTable(
                name: "platform_organization_memberships",
                newName: "edpf_orgmbr");

            migrationBuilder.RenameTable(
                name: "platform_invitations",
                newName: "edpf_inv");

            migrationBuilder.RenameTable(
                name: "platform_authorization_policy_changes",
                newName: "edpf_authplcy_chg");

            migrationBuilder.RenameTable(
                name: "platform_authorization_policies",
                newName: "edpf_authplcy");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit_retention_changes",
                newName: "edpf_authaud_rtnchg");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit_retention",
                newName: "edpf_authaud_rtn");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit_archive_schedules",
                newName: "edpf_authaud_arcschd");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit_archive_jobs",
                newName: "edpf_authaud_arcjob");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit_archive_attempts",
                newName: "edpf_authaud_arcattm");

            migrationBuilder.RenameTable(
                name: "platform_authorization_audit",
                newName: "edpf_authaud");

            migrationBuilder.RenameIndex(
                name: "IX_platform_users_TenantId_Email",
                table: "edpf_usr",
                newName: "IX_edpf_usr_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenants_Code",
                table: "edpf_tnt",
                newName: "IX_edpf_tnt_Code");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_memberships_TenantId_SubjectId",
                table: "edpf_tntmbr",
                newName: "IX_edpf_tntmbr_TenantId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind",
                table: "edpf_tntrtnalw",
                newName: "IX_edpf_tntrtnalw_TenantId_SchemaName_RoutineName_Kind");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey",
                table: "edpf_tntmut_aud",
                newName: "IX_edpf_tntmut_aud_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Status",
                table: "edpf_tntdbnd",
                newName: "IX_edpf_tntdbnd_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Provider",
                table: "edpf_tntdbnd",
                newName: "IX_edpf_tntdbnd_TenantId_Provider");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc",
                table: "edpf_strqta_plcychg",
                newName: "IX_edpf_strqta_plcychg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policy_changes_PolicyId_TenantId",
                table: "edpf_strqta_plcychg",
                newName: "IX_edpf_strqta_plcychg_PolicyId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policies_TenantId_Scope_TargetId",
                table: "edpf_strqta_plcy",
                newName: "IX_edpf_strqta_plcy_TenantId_Scope_TargetId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policies_TenantId_IsEnabled",
                table: "edpf_strqta_plcy",
                newName: "IX_edpf_strqta_plcy_TenantId_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_tenant_id",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_project_id_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_TenantId_OrganizationId",
                table: "edpf_prj",
                newName: "IX_edpf_prj_TenantId_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_OrganizationId_TenantId",
                table: "edpf_prj",
                newName: "IX_edpf_prj_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_OrganizationId_Code",
                table: "edpf_prj",
                newName: "IX_edpf_prj_OrganizationId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "edpf_prjmbr",
                newName: "IX_edpf_prjmbr_TenantId_OrganizationId_ProjectId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_project_memberships_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr",
                newName: "IX_edpf_prjmbr_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_tenant_id",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_project_id_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_Key_Version",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_Key_Version");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organizations_TenantId_Code",
                table: "edpf_org",
                newName: "IX_edpf_org_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organizations_TenantId",
                table: "edpf_org",
                newName: "IX_edpf_org_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId",
                table: "edpf_orgmbr",
                newName: "IX_edpf_orgmbr_TenantId_OrganizationId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organization_memberships_OrganizationId_TenantId",
                table: "edpf_orgmbr",
                newName: "IX_edpf_orgmbr_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_UserId",
                table: "edpf_inv",
                newName: "IX_edpf_inv_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TokenHash",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TenantId_UserId",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TenantId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TenantId_Status_CreatedAtUtc",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "IX_edpf_authplcy_chg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "IX_edpf_authplcy_chg_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_TenantId_SubjectId_Action_ResourceType_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc",
                table: "edpf_authaud_rtnchg",
                newName: "IX_edpf_authaud_rtnchg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_retention_TenantId",
                table: "edpf_authaud_rtn",
                newName: "IX_edpf_authaud_rtn_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_schedules_TenantId",
                table: "edpf_authaud_arcschd",
                newName: "IX_edpf_authaud_arcschd_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "IX_edpf_authaud_arcschd_IsEnabled_NextRunAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc",
                table: "edpf_authaud_arcjob",
                newName: "IX_edpf_authaud_arcjob_TenantId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc",
                table: "edpf_authaud_arcattm",
                newName: "IX_edpf_authaud_arcattm_TenantId_JobId_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_attempts_JobId_TenantId",
                table: "edpf_authaud_arcattm",
                newName: "IX_edpf_authaud_arcattm_JobId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_TenantId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "IX_edpf_authaud_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_SubjectId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "IX_edpf_authaud_SubjectId_OccurredAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_usr",
                table: "edpf_usr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tnt",
                table: "edpf_tnt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tntmbr",
                table: "edpf_tntmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tntrtnalw",
                table: "edpf_tntrtnalw",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tntmut_aud",
                table: "edpf_tntmut_aud",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tntdbnd",
                table: "edpf_tntdbnd",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_strqta_plcychg",
                table: "edpf_strqta_plcychg",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_strqta_plcy",
                table: "edpf_strqta_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_rsrc",
                table: "edpf_rsrc",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "Id", "OrganizationId", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_prj",
                table: "edpf_prj",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_prjmbr",
                table: "edpf_prjmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_plcy",
                table: "edpf_plcy",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_org",
                table: "edpf_org",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_orgmbr",
                table: "edpf_orgmbr",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_inv",
                table: "edpf_inv",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authplcy_chg",
                table: "edpf_authplcy_chg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authplcy",
                table: "edpf_authplcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud_rtnchg",
                table: "edpf_authaud_rtnchg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud_rtn",
                table: "edpf_authaud_rtn",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud_arcschd",
                table: "edpf_authaud_arcschd",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud_arcjob",
                table: "edpf_authaud_arcjob",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud_arcattm",
                table: "edpf_authaud_arcattm",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_authaud",
                table: "edpf_authaud",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_edpf_rsrc_project_requires_organization",
                table: "edpf_rsrc",
                sql: "project_id IS NULL OR organization_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_edpf_plcy_project_requires_organization",
                table: "edpf_plcy",
                sql: "project_id IS NULL OR organization_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_edpf_tnt_TenantId",
                table: "edpf_authaud",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_arcattm_edpf_authaud_arcjob_JobId_TenantId",
                table: "edpf_authaud_arcattm",
                columns: new[] { "JobId", "TenantId" },
                principalTable: "edpf_authaud_arcjob",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_arcattm_edpf_tnt_TenantId",
                table: "edpf_authaud_arcattm",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_arcjob_edpf_tnt_TenantId",
                table: "edpf_authaud_arcjob",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_arcschd_edpf_tnt_TenantId",
                table: "edpf_authaud_arcschd",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_rtn_edpf_tnt_TenantId",
                table: "edpf_authaud_rtn",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authaud_rtnchg_edpf_tnt_TenantId",
                table: "edpf_authaud_rtnchg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authplcy_edpf_org_OrganizationId_TenantId",
                table: "edpf_authplcy",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authplcy_edpf_prj_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authplcy_edpf_tnt_TenantId",
                table: "edpf_authplcy",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_authplcy_chg_edpf_tnt_TenantId",
                table: "edpf_authplcy_chg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_inv_edpf_tnt_TenantId",
                table: "edpf_inv",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_inv_edpf_usr_UserId",
                table: "edpf_inv",
                column: "UserId",
                principalTable: "edpf_usr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_org_edpf_tnt_TenantId",
                table: "edpf_org",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_orgmbr_edpf_org_OrganizationId_TenantId",
                table: "edpf_orgmbr",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_plcy_edpf_org_organization_id_tenant_id",
                table: "edpf_plcy",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_plcy_edpf_prj_project_id_organization_id_tenant_id",
                table: "edpf_plcy",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_plcy_edpf_tnt_tenant_id",
                table: "edpf_plcy",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_prj_edpf_org_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_prj_edpf_tnt_TenantId",
                table: "edpf_prj",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_prjmbr_edpf_prj_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_rsrc_edpf_org_organization_id_tenant_id",
                table: "edpf_rsrc",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_rsrc_edpf_prj_project_id_organization_id_tenant_id",
                table: "edpf_rsrc",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_rsrc_edpf_tnt_tenant_id",
                table: "edpf_rsrc",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_strqta_plcy_edpf_tnt_TenantId",
                table: "edpf_strqta_plcy",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_strqta_plcychg_edpf_strqta_plcy_PolicyId_TenantId",
                table: "edpf_strqta_plcychg",
                columns: new[] { "PolicyId", "TenantId" },
                principalTable: "edpf_strqta_plcy",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_strqta_plcychg_edpf_tnt_TenantId",
                table: "edpf_strqta_plcychg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_tntdbnd_edpf_tnt_TenantId",
                table: "edpf_tntdbnd",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_tntmbr_edpf_tnt_TenantId",
                table: "edpf_tntmbr",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_tntmut_aud_edpf_tnt_TenantId",
                table: "edpf_tntmut_aud",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_tntrtnalw_edpf_tnt_TenantId",
                table: "edpf_tntrtnalw",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edpf_usr_edpf_tnt_TenantId",
                table: "edpf_usr",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_edpf_tnt_TenantId",
                table: "edpf_authaud");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_arcattm_edpf_authaud_arcjob_JobId_TenantId",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_arcattm_edpf_tnt_TenantId",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_arcjob_edpf_tnt_TenantId",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_arcschd_edpf_tnt_TenantId",
                table: "edpf_authaud_arcschd");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_rtn_edpf_tnt_TenantId",
                table: "edpf_authaud_rtn");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authaud_rtnchg_edpf_tnt_TenantId",
                table: "edpf_authaud_rtnchg");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authplcy_edpf_org_OrganizationId_TenantId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authplcy_edpf_prj_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authplcy_edpf_tnt_TenantId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_authplcy_chg_edpf_tnt_TenantId",
                table: "edpf_authplcy_chg");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_inv_edpf_tnt_TenantId",
                table: "edpf_inv");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_inv_edpf_usr_UserId",
                table: "edpf_inv");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_org_edpf_tnt_TenantId",
                table: "edpf_org");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_orgmbr_edpf_org_OrganizationId_TenantId",
                table: "edpf_orgmbr");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_plcy_edpf_org_organization_id_tenant_id",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_plcy_edpf_prj_project_id_organization_id_tenant_id",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_plcy_edpf_tnt_tenant_id",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_prj_edpf_org_OrganizationId_TenantId",
                table: "edpf_prj");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_prj_edpf_tnt_TenantId",
                table: "edpf_prj");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_prjmbr_edpf_prj_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_rsrc_edpf_org_organization_id_tenant_id",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_rsrc_edpf_prj_project_id_organization_id_tenant_id",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_rsrc_edpf_tnt_tenant_id",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_strqta_plcy_edpf_tnt_TenantId",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_strqta_plcychg_edpf_strqta_plcy_PolicyId_TenantId",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_strqta_plcychg_edpf_tnt_TenantId",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_tntdbnd_edpf_tnt_TenantId",
                table: "edpf_tntdbnd");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_tntmbr_edpf_tnt_TenantId",
                table: "edpf_tntmbr");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_tntmut_aud_edpf_tnt_TenantId",
                table: "edpf_tntmut_aud");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_tntrtnalw_edpf_tnt_TenantId",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropForeignKey(
                name: "FK_edpf_usr_edpf_tnt_TenantId",
                table: "edpf_usr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_usr",
                table: "edpf_usr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_tntrtnalw",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_tntmut_aud",
                table: "edpf_tntmut_aud");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_tntmbr",
                table: "edpf_tntmbr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_tntdbnd",
                table: "edpf_tntdbnd");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_tnt",
                table: "edpf_tnt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_strqta_plcychg",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_strqta_plcy",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_rsrc",
                table: "edpf_rsrc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_edpf_rsrc_project_requires_organization",
                table: "edpf_rsrc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_prjmbr",
                table: "edpf_prjmbr");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_prj",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_plcy",
                table: "edpf_plcy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_edpf_plcy_project_requires_organization",
                table: "edpf_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_orgmbr",
                table: "edpf_orgmbr");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_org",
                table: "edpf_org");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_inv",
                table: "edpf_inv");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authplcy_chg",
                table: "edpf_authplcy_chg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authplcy",
                table: "edpf_authplcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_rtnchg",
                table: "edpf_authaud_rtnchg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_rtn",
                table: "edpf_authaud_rtn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_arcschd",
                table: "edpf_authaud_arcschd");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_arcjob",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_arcattm",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud",
                table: "edpf_authaud");

            migrationBuilder.RenameTable(
                name: "edpf_usr",
                newName: "platform_users");

            migrationBuilder.RenameTable(
                name: "edpf_tntrtnalw",
                newName: "platform_tenant_database_routine_allowlists");

            migrationBuilder.RenameTable(
                name: "edpf_tntmut_aud",
                newName: "platform_tenant_database_mutation_audits");

            migrationBuilder.RenameTable(
                name: "edpf_tntmbr",
                newName: "platform_tenant_memberships");

            migrationBuilder.RenameTable(
                name: "edpf_tntdbnd",
                newName: "platform_tenant_database_bindings");

            migrationBuilder.RenameTable(
                name: "edpf_tnt",
                newName: "platform_tenants");

            migrationBuilder.RenameTable(
                name: "edpf_strqta_plcychg",
                newName: "platform_storage_quota_policy_changes");

            migrationBuilder.RenameTable(
                name: "edpf_strqta_plcy",
                newName: "platform_storage_quota_policies");

            migrationBuilder.RenameTable(
                name: "edpf_rsrc",
                newName: "platform_resources");

            migrationBuilder.RenameTable(
                name: "edpf_prjmbr",
                newName: "platform_project_memberships");

            migrationBuilder.RenameTable(
                name: "edpf_prj",
                newName: "platform_projects");

            migrationBuilder.RenameTable(
                name: "edpf_plcy",
                newName: "platform_policies");

            migrationBuilder.RenameTable(
                name: "edpf_orgmbr",
                newName: "platform_organization_memberships");

            migrationBuilder.RenameTable(
                name: "edpf_org",
                newName: "platform_organizations");

            migrationBuilder.RenameTable(
                name: "edpf_inv",
                newName: "platform_invitations");

            migrationBuilder.RenameTable(
                name: "edpf_authplcy_chg",
                newName: "platform_authorization_policy_changes");

            migrationBuilder.RenameTable(
                name: "edpf_authplcy",
                newName: "platform_authorization_policies");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_rtnchg",
                newName: "platform_authorization_audit_retention_changes");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_rtn",
                newName: "platform_authorization_audit_retention");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcschd",
                newName: "platform_authorization_audit_archive_schedules");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcjob",
                newName: "platform_authorization_audit_archive_jobs");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcattm",
                newName: "platform_authorization_audit_archive_attempts");

            migrationBuilder.RenameTable(
                name: "edpf_authaud",
                newName: "platform_authorization_audit");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_usr_TenantId_Email",
                table: "platform_users",
                newName: "IX_platform_users_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntrtnalw_TenantId_SchemaName_RoutineName_Kind",
                table: "platform_tenant_database_routine_allowlists",
                newName: "IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntmut_aud_TenantId_IdempotencyKey",
                table: "platform_tenant_database_mutation_audits",
                newName: "IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntmbr_TenantId_SubjectId",
                table: "platform_tenant_memberships",
                newName: "IX_platform_tenant_memberships_TenantId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntdbnd_TenantId_Status",
                table: "platform_tenant_database_bindings",
                newName: "IX_platform_tenant_database_bindings_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntdbnd_TenantId_Provider",
                table: "platform_tenant_database_bindings",
                newName: "IX_platform_tenant_database_bindings_TenantId_Provider");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tnt_Code",
                table: "platform_tenants",
                newName: "IX_platform_tenants_Code");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcychg_TenantId_OccurredAtUtc",
                table: "platform_storage_quota_policy_changes",
                newName: "IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcychg_PolicyId_TenantId",
                table: "platform_storage_quota_policy_changes",
                newName: "IX_platform_storage_quota_policy_changes_PolicyId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcy_TenantId_Scope_TargetId",
                table: "platform_storage_quota_policies",
                newName: "IX_platform_storage_quota_policies_TenantId_Scope_TargetId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcy_TenantId_IsEnabled",
                table: "platform_storage_quota_policies",
                newName: "IX_platform_storage_quota_policies_TenantId_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_tenant_id",
                table: "platform_resources",
                newName: "IX_platform_resources_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_project_id_organization_id_tenant_id",
                table: "platform_resources",
                newName: "IX_platform_resources_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_organization_id_tenant_id",
                table: "platform_resources",
                newName: "IX_platform_resources_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prjmbr_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "platform_project_memberships",
                newName: "IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prjmbr_ProjectId_OrganizationId_TenantId",
                table: "platform_project_memberships",
                newName: "IX_platform_project_memberships_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_TenantId_OrganizationId",
                table: "platform_projects",
                newName: "IX_platform_projects_TenantId_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_OrganizationId_TenantId",
                table: "platform_projects",
                newName: "IX_platform_projects_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_OrganizationId_Code",
                table: "platform_projects",
                newName: "IX_platform_projects_OrganizationId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_tenant_id",
                table: "platform_policies",
                newName: "IX_platform_policies_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_project_id_organization_id_tenant_id",
                table: "platform_policies",
                newName: "IX_platform_policies_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_organization_id_tenant_id",
                table: "platform_policies",
                newName: "IX_platform_policies_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_Key_Version",
                table: "platform_policies",
                newName: "IX_platform_policies_Key_Version");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_orgmbr_TenantId_OrganizationId_SubjectId",
                table: "platform_organization_memberships",
                newName: "IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_orgmbr_OrganizationId_TenantId",
                table: "platform_organization_memberships",
                newName: "IX_platform_organization_memberships_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_org_TenantId_Code",
                table: "platform_organizations",
                newName: "IX_platform_organizations_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_org_TenantId",
                table: "platform_organizations",
                newName: "IX_platform_organizations_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_UserId",
                table: "platform_invitations",
                newName: "IX_platform_invitations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TokenHash",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TenantId_UserId",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TenantId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TenantId_Status_CreatedAtUtc",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_chg_TenantId_OccurredAtUtc",
                table: "platform_authorization_policy_changes",
                newName: "IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_chg_PolicyId_OccurredAtUtc",
                table: "platform_authorization_policy_changes",
                newName: "IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_TenantId_SubjectId_Action_ResourceType_IsEnabled",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_ProjectId_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_rtnchg_TenantId_OccurredAtUtc",
                table: "platform_authorization_audit_retention_changes",
                newName: "IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_rtn_TenantId",
                table: "platform_authorization_audit_retention",
                newName: "IX_platform_authorization_audit_retention_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcschd_TenantId",
                table: "platform_authorization_audit_archive_schedules",
                newName: "IX_platform_authorization_audit_archive_schedules_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcschd_IsEnabled_NextRunAtUtc",
                table: "platform_authorization_audit_archive_schedules",
                newName: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcjob_TenantId_CreatedAtUtc",
                table: "platform_authorization_audit_archive_jobs",
                newName: "IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcattm_TenantId_JobId_StartedAtUtc",
                table: "platform_authorization_audit_archive_attempts",
                newName: "IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcattm_JobId_TenantId",
                table: "platform_authorization_audit_archive_attempts",
                newName: "IX_platform_authorization_audit_archive_attempts_JobId_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_TenantId_OccurredAtUtc",
                table: "platform_authorization_audit",
                newName: "IX_platform_authorization_audit_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_SubjectId_OccurredAtUtc",
                table: "platform_authorization_audit",
                newName: "IX_platform_authorization_audit_SubjectId_OccurredAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_users",
                table: "platform_users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_tenant_database_routine_allowlists",
                table: "platform_tenant_database_routine_allowlists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_tenant_database_mutation_audits",
                table: "platform_tenant_database_mutation_audits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_tenant_memberships",
                table: "platform_tenant_memberships",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_tenant_database_bindings",
                table: "platform_tenant_database_bindings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_tenants",
                table: "platform_tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_storage_quota_policy_changes",
                table: "platform_storage_quota_policy_changes",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_platform_storage_quota_policies_Id_TenantId",
                table: "platform_storage_quota_policies",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_storage_quota_policies",
                table: "platform_storage_quota_policies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_resources",
                table: "platform_resources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_project_memberships",
                table: "platform_project_memberships",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_platform_projects_Id_OrganizationId_TenantId",
                table: "platform_projects",
                columns: new[] { "Id", "OrganizationId", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_projects",
                table: "platform_projects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_policies",
                table: "platform_policies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_organization_memberships",
                table: "platform_organization_memberships",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_platform_organizations_Id_TenantId",
                table: "platform_organizations",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_organizations",
                table: "platform_organizations",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_platform_invitations_Id_TenantId",
                table: "platform_invitations",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_invitations",
                table: "platform_invitations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_policy_changes",
                table: "platform_authorization_policy_changes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_policies",
                table: "platform_authorization_policies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit_retention_changes",
                table: "platform_authorization_audit_retention_changes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit_retention",
                table: "platform_authorization_audit_retention",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit_archive_schedules",
                table: "platform_authorization_audit_archive_schedules",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_platform_authorization_audit_archive_jobs_Id_TenantId",
                table: "platform_authorization_audit_archive_jobs",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit_archive_jobs",
                table: "platform_authorization_audit_archive_jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit_archive_attempts",
                table: "platform_authorization_audit_archive_attempts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_authorization_audit",
                table: "platform_authorization_audit",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_platform_resources_project_requires_organization",
                table: "platform_resources",
                sql: "project_id IS NULL OR organization_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_platform_policies_project_requires_organization",
                table: "platform_policies",
                sql: "project_id IS NULL OR organization_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_platform_tenants_TenantId",
                table: "platform_authorization_audit",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_archive_attempts_platform_authorization_audit_archive_jobs_JobId_TenantId",
                table: "platform_authorization_audit_archive_attempts",
                columns: new[] { "JobId", "TenantId" },
                principalTable: "platform_authorization_audit_archive_jobs",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_archive_attempts_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_attempts",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_archive_jobs_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_jobs",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_archive_schedules_platform_tenants_TenantId",
                table: "platform_authorization_audit_archive_schedules",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_retention_platform_tenants_TenantId",
                table: "platform_authorization_audit_retention",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_audit_retention_changes_platform_tenants_TenantId",
                table: "platform_authorization_audit_retention_changes",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_policies_platform_organizations_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "platform_organizations",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_policies_platform_projects_ProjectId_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "platform_projects",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_policies_platform_tenants_TenantId",
                table: "platform_authorization_policies",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_authorization_policy_changes_platform_tenants_TenantId",
                table: "platform_authorization_policy_changes",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_invitations_platform_tenants_TenantId",
                table: "platform_invitations",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_invitations_platform_users_UserId",
                table: "platform_invitations",
                column: "UserId",
                principalTable: "platform_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_organization_memberships_platform_organizations_OrganizationId_TenantId",
                table: "platform_organization_memberships",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "platform_organizations",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_organizations_platform_tenants_TenantId",
                table: "platform_organizations",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_policies_platform_organizations_organization_id_tenant_id",
                table: "platform_policies",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "platform_organizations",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_policies_platform_projects_project_id_organization_id_tenant_id",
                table: "platform_policies",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "platform_projects",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_policies_platform_tenants_tenant_id",
                table: "platform_policies",
                column: "tenant_id",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_project_memberships_platform_projects_ProjectId_OrganizationId_TenantId",
                table: "platform_project_memberships",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "platform_projects",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_projects_platform_organizations_OrganizationId_TenantId",
                table: "platform_projects",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "platform_organizations",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_projects_platform_tenants_TenantId",
                table: "platform_projects",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_resources_platform_organizations_organization_id_tenant_id",
                table: "platform_resources",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "platform_organizations",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_resources_platform_projects_project_id_organization_id_tenant_id",
                table: "platform_resources",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "platform_projects",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_resources_platform_tenants_tenant_id",
                table: "platform_resources",
                column: "tenant_id",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_storage_quota_policies_platform_tenants_TenantId",
                table: "platform_storage_quota_policies",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_storage_quota_policy_changes_platform_storage_quota_policies_PolicyId_TenantId",
                table: "platform_storage_quota_policy_changes",
                columns: new[] { "PolicyId", "TenantId" },
                principalTable: "platform_storage_quota_policies",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_storage_quota_policy_changes_platform_tenants_TenantId",
                table: "platform_storage_quota_policy_changes",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_tenant_database_bindings_platform_tenants_TenantId",
                table: "platform_tenant_database_bindings",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId",
                table: "platform_tenant_database_mutation_audits",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId",
                table: "platform_tenant_database_routine_allowlists",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_tenant_memberships_platform_tenants_TenantId",
                table: "platform_tenant_memberships",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_users_platform_tenants_TenantId",
                table: "platform_users",
                column: "TenantId",
                principalTable: "platform_tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

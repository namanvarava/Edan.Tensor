using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
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
                name: "FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId",
                table: "platform_tenant_database_routine_allowlists");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_memberships_platform_tenants_TenantId",
                table: "platform_tenant_memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_users_platform_tenants_TenantId",
                table: "platform_users");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId",
                table: "platform_tenant_database_mutation_audits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_users",
                table: "platform_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_tenant_database_mutation_audits",
                table: "platform_tenant_database_mutation_audits");

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
                name: "platform_tenant_database_mutation_audits",
                newName: "edpf_tntmut_aud");

            migrationBuilder.RenameTable(
                name: "platform_tenant_database_routine_allowlists",
                newName: "edpf_tntrtnalw");

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
                newName: "edpf_usr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenants_Code",
                table: "edpf_tnt",
                newName: "edpf_tnt_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_memberships_TenantId_SubjectId",
                table: "edpf_tntmbr",
                newName: "edpf_tntmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind",
                table: "edpf_tntrtnalw",
                newName: "edpf_tntrtnalw_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Status",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Provider",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policy_changes_PolicyId_TenantId",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policies_TenantId_Scope_TargetId",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_storage_quota_policies_TenantId_IsEnabled",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_project_id_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N03");

            migrationBuilder.RenameIndex(
                name: "IX_platform_resources_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_TenantId_OrganizationId",
                table: "edpf_prj",
                newName: "edpf_prj_N03");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_OrganizationId_TenantId",
                table: "edpf_prj",
                newName: "edpf_prj_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_projects_OrganizationId_Code",
                table: "edpf_prj",
                newName: "edpf_prj_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_project_memberships_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_project_id_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N04");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N03");

            migrationBuilder.RenameIndex(
                name: "IX_platform_policies_Key_Version",
                table: "edpf_plcy",
                newName: "edpf_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organizations_TenantId_Code",
                table: "edpf_org",
                newName: "edpf_org_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organizations_TenantId",
                table: "edpf_org",
                newName: "edpf_org_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_organization_memberships_OrganizationId_TenantId",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_UserId",
                table: "edpf_inv",
                newName: "edpf_inv_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TokenHash",
                table: "edpf_inv",
                newName: "edpf_inv_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TenantId_UserId",
                table: "edpf_inv",
                newName: "edpf_inv_N03");

            migrationBuilder.RenameIndex(
                name: "IX_platform_invitations_TenantId_Status_CreatedAtUtc",
                table: "edpf_inv",
                newName: "edpf_inv_N04");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N03");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_policies_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc",
                table: "edpf_authaud_rtnchg",
                newName: "edpf_authaud_rtnchg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_retention_TenantId",
                table: "edpf_authaud_rtn",
                newName: "edpf_authaud_rtn_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_schedules_TenantId",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc",
                table: "edpf_authaud_arcjob",
                newName: "edpf_authaud_arcjob_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_archive_attempts_JobId_TenantId",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_TenantId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "edpf_authaud_N02");

            migrationBuilder.RenameIndex(
                name: "IX_platform_authorization_audit_SubjectId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "edpf_authaud_N01");

            migrationBuilder.RenameIndex(
                name: "IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey",
                table: "edpf_tntmut_aud",
                newName: "edpf_tntmut_aud_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_usr_PK",
                table: "edpf_usr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmut_aud_PK",
                table: "edpf_tntmut_aud",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tnt_PK",
                table: "edpf_tnt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmbr_PK",
                table: "edpf_tntmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntrtnalw_PK",
                table: "edpf_tntrtnalw",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntdbnd_PK",
                table: "edpf_tntdbnd",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcychg_PK",
                table: "edpf_strqta_plcychg",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "Id", "OrganizationId", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_org_PK",
                table: "edpf_org",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_inv_PK",
                table: "edpf_inv",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authplcy_chg_PK",
                table: "edpf_authplcy_chg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authplcy_PK",
                table: "edpf_authplcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_rtnchg_PK",
                table: "edpf_authaud_rtnchg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_rtn_PK",
                table: "edpf_authaud_rtn",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcschd_PK",
                table: "edpf_authaud_arcschd",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcjob_PK",
                table: "edpf_authaud_arcjob",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcattm_PK",
                table: "edpf_authaud_arcattm",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_PK",
                table: "edpf_authaud",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "edpf_tntmut_aud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Succeeded = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    Compensated = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    ResultJson = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Error = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edpf_tntmut_aud_PK", x => x.Id);
                    table.ForeignKey(
                        name: "edpf_tntmut_aud_FKTenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_edpf_rsrc_project_requires_organization",
                table: "edpf_rsrc",
                sql: "\"project_id\" IS NULL OR \"organization_id\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_edpf_plcy_project_requires_organization",
                table: "edpf_plcy",
                sql: "\"project_id\" IS NULL OR \"organization_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "edpf_tntmut_aud_N01",
                table: "edpf_tntmut_aud",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntmut_aud_FKTenantId",
                table: "edpf_tntmut_aud",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_FKTenantId",
                table: "edpf_authaud",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcattm_FKJobId",
                table: "edpf_authaud_arcattm",
                columns: new[] { "JobId", "TenantId" },
                principalTable: "edpf_authaud_arcjob",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcattm_FKTenantId",
                table: "edpf_authaud_arcattm",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcjob_FKTenantId",
                table: "edpf_authaud_arcjob",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcschd_FKTenantId",
                table: "edpf_authaud_arcschd",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_rtn_FKTenantId",
                table: "edpf_authaud_rtn",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_rtnchg_FKTenantId",
                table: "edpf_authaud_rtnchg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKOrganizationId",
                table: "edpf_authplcy",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKProjectId",
                table: "edpf_authplcy",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKTenantId",
                table: "edpf_authplcy",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_chg_FKTenantId",
                table: "edpf_authplcy_chg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_inv_FKTenantId",
                table: "edpf_inv",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_inv_FKUserId",
                table: "edpf_inv",
                column: "UserId",
                principalTable: "edpf_usr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_org_FKTenantId",
                table: "edpf_org",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_orgmbr_FKOrganizationId",
                table: "edpf_orgmbr",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeOrganizationId",
                table: "edpf_plcy",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeProjectId",
                table: "edpf_plcy",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeTenantId",
                table: "edpf_plcy",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prj_FKOrganizationId",
                table: "edpf_prj",
                columns: new[] { "OrganizationId", "TenantId" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prj_FKTenantId",
                table: "edpf_prj",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prjmbr_FKProjectId",
                table: "edpf_prjmbr",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeOrganizationId",
                table: "edpf_rsrc",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeProjectId",
                table: "edpf_rsrc",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeTenantId",
                table: "edpf_rsrc",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcy_FKTenantId",
                table: "edpf_strqta_plcy",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcychg_FKPolicyId",
                table: "edpf_strqta_plcychg",
                columns: new[] { "PolicyId", "TenantId" },
                principalTable: "edpf_strqta_plcy",
                principalColumns: new[] { "Id", "TenantId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcychg_FKTenantId",
                table: "edpf_strqta_plcychg",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntdbnd_FKTenantId",
                table: "edpf_tntdbnd",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntmbr_FKTenantId",
                table: "edpf_tntmbr",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntrtnalw_FKTenantId",
                table: "edpf_tntrtnalw",
                column: "TenantId",
                principalTable: "edpf_tnt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_usr_FKTenantId",
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
                name: "edpf_authaud_FKTenantId",
                table: "edpf_authaud");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_arcattm_FKJobId",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_arcattm_FKTenantId",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_arcjob_FKTenantId",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_arcschd_FKTenantId",
                table: "edpf_authaud_arcschd");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_rtn_FKTenantId",
                table: "edpf_authaud_rtn");

            migrationBuilder.DropForeignKey(
                name: "edpf_authaud_rtnchg_FKTenantId",
                table: "edpf_authaud_rtnchg");

            migrationBuilder.DropForeignKey(
                name: "edpf_authplcy_FKOrganizationId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_authplcy_FKProjectId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_authplcy_FKTenantId",
                table: "edpf_authplcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_authplcy_chg_FKTenantId",
                table: "edpf_authplcy_chg");

            migrationBuilder.DropForeignKey(
                name: "edpf_inv_FKTenantId",
                table: "edpf_inv");

            migrationBuilder.DropForeignKey(
                name: "edpf_inv_FKUserId",
                table: "edpf_inv");

            migrationBuilder.DropForeignKey(
                name: "edpf_org_FKTenantId",
                table: "edpf_org");

            migrationBuilder.DropForeignKey(
                name: "edpf_orgmbr_FKOrganizationId",
                table: "edpf_orgmbr");

            migrationBuilder.DropForeignKey(
                name: "edpf_plcy_FKScopeOrganizationId",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_plcy_FKScopeProjectId",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_plcy_FKScopeTenantId",
                table: "edpf_plcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_prj_FKOrganizationId",
                table: "edpf_prj");

            migrationBuilder.DropForeignKey(
                name: "edpf_prj_FKTenantId",
                table: "edpf_prj");

            migrationBuilder.DropForeignKey(
                name: "edpf_prjmbr_FKProjectId",
                table: "edpf_prjmbr");

            migrationBuilder.DropForeignKey(
                name: "edpf_rsrc_FKScopeOrganizationId",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "edpf_rsrc_FKScopeProjectId",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "edpf_rsrc_FKScopeTenantId",
                table: "edpf_rsrc");

            migrationBuilder.DropForeignKey(
                name: "edpf_strqta_plcy_FKTenantId",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropForeignKey(
                name: "edpf_strqta_plcychg_FKPolicyId",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropForeignKey(
                name: "edpf_strqta_plcychg_FKTenantId",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropForeignKey(
                name: "edpf_tntdbnd_FKTenantId",
                table: "edpf_tntdbnd");

            migrationBuilder.DropForeignKey(
                name: "edpf_tntmbr_FKTenantId",
                table: "edpf_tntmbr");

            migrationBuilder.DropForeignKey(
                name: "edpf_tntrtnalw_FKTenantId",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropForeignKey(
                name: "edpf_usr_FKTenantId",
                table: "edpf_usr");

            migrationBuilder.DropTable(
                name: "edpf_tntmut_aud");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_usr_PK",
                table: "edpf_usr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tntrtnalw_PK",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tntmbr_PK",
                table: "edpf_tntmbr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tntdbnd_PK",
                table: "edpf_tntdbnd");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tnt_PK",
                table: "edpf_tnt");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_strqta_plcychg_PK",
                table: "edpf_strqta_plcychg");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_edpf_rsrc_project_requires_organization",
                table: "edpf_rsrc");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_edpf_plcy_project_requires_organization",
                table: "edpf_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_org_PK",
                table: "edpf_org");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_inv_PK",
                table: "edpf_inv");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authplcy_chg_PK",
                table: "edpf_authplcy_chg");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authplcy_PK",
                table: "edpf_authplcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_rtnchg_PK",
                table: "edpf_authaud_rtnchg");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_rtn_PK",
                table: "edpf_authaud_rtn");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_arcschd_PK",
                table: "edpf_authaud_arcschd");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_arcjob_PK",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_arcattm_PK",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_PK",
                table: "edpf_authaud");

            migrationBuilder.RenameTable(
                name: "edpf_usr",
                newName: "platform_users");

            migrationBuilder.RenameTable(
                name: "edpf_tntrtnalw",
                newName: "platform_tenant_database_routine_allowlists");

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
                name: "edpf_usr_N01",
                table: "platform_users",
                newName: "IX_platform_users_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "edpf_tntrtnalw_N01",
                table: "platform_tenant_database_routine_allowlists",
                newName: "IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind");

            migrationBuilder.RenameIndex(
                name: "edpf_tntmbr_N01",
                table: "platform_tenant_memberships",
                newName: "IX_platform_tenant_memberships_TenantId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N02",
                table: "platform_tenant_database_bindings",
                newName: "IX_platform_tenant_database_bindings_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N01",
                table: "platform_tenant_database_bindings",
                newName: "IX_platform_tenant_database_bindings_TenantId_Provider");

            migrationBuilder.RenameIndex(
                name: "edpf_tnt_N01",
                table: "platform_tenants",
                newName: "IX_platform_tenants_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N02",
                table: "platform_storage_quota_policy_changes",
                newName: "IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N01",
                table: "platform_storage_quota_policy_changes",
                newName: "IX_platform_storage_quota_policy_changes_PolicyId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N02",
                table: "platform_storage_quota_policies",
                newName: "IX_platform_storage_quota_policies_TenantId_Scope_TargetId");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N01",
                table: "platform_storage_quota_policies",
                newName: "IX_platform_storage_quota_policies_TenantId_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N03",
                table: "platform_resources",
                newName: "IX_platform_resources_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N02",
                table: "platform_resources",
                newName: "IX_platform_resources_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N01",
                table: "platform_resources",
                newName: "IX_platform_resources_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N02",
                table: "platform_project_memberships",
                newName: "IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N01",
                table: "platform_project_memberships",
                newName: "IX_platform_project_memberships_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N03",
                table: "platform_projects",
                newName: "IX_platform_projects_TenantId_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N02",
                table: "platform_projects",
                newName: "IX_platform_projects_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N01",
                table: "platform_projects",
                newName: "IX_platform_projects_OrganizationId_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N04",
                table: "platform_policies",
                newName: "IX_platform_policies_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N03",
                table: "platform_policies",
                newName: "IX_platform_policies_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N02",
                table: "platform_policies",
                newName: "IX_platform_policies_Key_Version");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N01",
                table: "platform_policies",
                newName: "IX_platform_policies_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N02",
                table: "platform_organization_memberships",
                newName: "IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N01",
                table: "platform_organization_memberships",
                newName: "IX_platform_organization_memberships_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N02",
                table: "platform_organizations",
                newName: "IX_platform_organizations_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N01",
                table: "platform_organizations",
                newName: "IX_platform_organizations_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N04",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N03",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TenantId_UserId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N02",
                table: "platform_invitations",
                newName: "IX_platform_invitations_UserId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N01",
                table: "platform_invitations",
                newName: "IX_platform_invitations_TokenHash");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N02",
                table: "platform_authorization_policy_changes",
                newName: "IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N01",
                table: "platform_authorization_policy_changes",
                newName: "IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N03",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N02",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N01",
                table: "platform_authorization_policies",
                newName: "IX_platform_authorization_policies_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtnchg_N01",
                table: "platform_authorization_audit_retention_changes",
                newName: "IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtn_N01",
                table: "platform_authorization_audit_retention",
                newName: "IX_platform_authorization_audit_retention_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N02",
                table: "platform_authorization_audit_archive_schedules",
                newName: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N01",
                table: "platform_authorization_audit_archive_schedules",
                newName: "IX_platform_authorization_audit_archive_schedules_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcjob_N01",
                table: "platform_authorization_audit_archive_jobs",
                newName: "IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N02",
                table: "platform_authorization_audit_archive_attempts",
                newName: "IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N01",
                table: "platform_authorization_audit_archive_attempts",
                newName: "IX_platform_authorization_audit_archive_attempts_JobId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N02",
                table: "platform_authorization_audit",
                newName: "IX_platform_authorization_audit_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N01",
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

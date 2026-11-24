using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_strqta_plcy",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_rsrc",
                table: "edpf_rsrc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_prjmbr",
                table: "edpf_prjmbr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_prj",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_plcy",
                table: "edpf_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_orgmbr",
                table: "edpf_orgmbr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_org",
                table: "edpf_org");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_arcjob",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud_arcattm",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edpf_authaud",
                table: "edpf_authaud");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_usr_TenantId_Email",
                table: "edpf_usr",
                newName: "edpf_usr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntrtnalw_TenantId_SchemaName_RoutineName_Kind",
                table: "edpf_tntrtnalw",
                newName: "edpf_tntrtnalw_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntmut_aud_TenantId_IdempotencyKey",
                table: "edpf_tntmut_aud",
                newName: "edpf_tntmut_aud_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntmbr_TenantId_SubjectId",
                table: "edpf_tntmbr",
                newName: "edpf_tntmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntdbnd_TenantId_Status",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tntdbnd_TenantId_Provider",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_tnt_Code",
                table: "edpf_tnt",
                newName: "edpf_tnt_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcychg_TenantId_OccurredAtUtc",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcychg_PolicyId_TenantId",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcy_TenantId_Scope_TargetId",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_strqta_plcy_TenantId_IsEnabled",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_project_id_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_rsrc_organization_id_tenant_id",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prjmbr_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prjmbr_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_TenantId_OrganizationId",
                table: "edpf_prj",
                newName: "edpf_prj_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_OrganizationId_TenantId",
                table: "edpf_prj",
                newName: "edpf_prj_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_prj_OrganizationId_Code",
                table: "edpf_prj",
                newName: "edpf_prj_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_project_id_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N04");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_organization_id_tenant_id",
                table: "edpf_plcy",
                newName: "edpf_plcy_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_plcy_Key_Version",
                table: "edpf_plcy",
                newName: "edpf_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_orgmbr_TenantId_OrganizationId_SubjectId",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_orgmbr_OrganizationId_TenantId",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_org_TenantId_Code",
                table: "edpf_org",
                newName: "edpf_org_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_org_TenantId",
                table: "edpf_org",
                newName: "edpf_org_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_UserId",
                table: "edpf_inv",
                newName: "edpf_inv_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TokenHash",
                table: "edpf_inv",
                newName: "edpf_inv_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TenantId_UserId",
                table: "edpf_inv",
                newName: "edpf_inv_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_inv_TenantId_Status_CreatedAtUtc",
                table: "edpf_inv",
                newName: "edpf_inv_N04");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_chg_TenantId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_chg_PolicyId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_TenantId_SubjectId_Action_ResourceType_IsEnabled",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authplcy_OrganizationId_TenantId",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_rtnchg_TenantId_OccurredAtUtc",
                table: "edpf_authaud_rtnchg",
                newName: "edpf_authaud_rtnchg_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_rtn_TenantId",
                table: "edpf_authaud_rtn",
                newName: "edpf_authaud_rtn_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcschd_TenantId",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcschd_IsEnabled_NextRunAtUtc",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcjob_TenantId_CreatedAtUtc",
                table: "edpf_authaud_arcjob",
                newName: "edpf_authaud_arcjob_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcattm_TenantId_JobId_StartedAtUtc",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_arcattm_JobId_TenantId",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_TenantId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "edpf_authaud_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edpf_authaud_SubjectId_OccurredAtUtc",
                table: "edpf_authaud",
                newName: "edpf_authaud_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_usr_PK",
                table: "edpf_usr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntrtnalw_PK",
                table: "edpf_tntrtnalw",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmut_aud_PK",
                table: "edpf_tntmut_aud",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmbr_PK",
                table: "edpf_tntmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntdbnd_PK",
                table: "edpf_tntdbnd",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tnt_PK",
                table: "edpf_tnt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcychg_PK",
                table: "edpf_strqta_plcychg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_org_PK",
                table: "edpf_org",
                column: "Id");

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
                name: "edpf_tntmut_aud_FKTenantId",
                table: "edpf_tntmut_aud",
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
                name: "edpf_tntmut_aud_FKTenantId",
                table: "edpf_tntmut_aud");

            migrationBuilder.DropForeignKey(
                name: "edpf_tntrtnalw_FKTenantId",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropForeignKey(
                name: "edpf_usr_FKTenantId",
                table: "edpf_usr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_usr_PK",
                table: "edpf_usr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tntrtnalw_PK",
                table: "edpf_tntrtnalw");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_tntmut_aud_PK",
                table: "edpf_tntmut_aud");

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

            migrationBuilder.DropPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_org_PK",
                table: "edpf_org");

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

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_arcjob_PK",
                table: "edpf_authaud_arcjob");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_arcattm_PK",
                table: "edpf_authaud_arcattm");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_authaud_PK",
                table: "edpf_authaud");

            migrationBuilder.RenameIndex(
                name: "edpf_usr_N01",
                table: "edpf_usr",
                newName: "IX_edpf_usr_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "edpf_tntrtnalw_N01",
                table: "edpf_tntrtnalw",
                newName: "IX_edpf_tntrtnalw_TenantId_SchemaName_RoutineName_Kind");

            migrationBuilder.RenameIndex(
                name: "edpf_tntmut_aud_N01",
                table: "edpf_tntmut_aud",
                newName: "IX_edpf_tntmut_aud_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edpf_tntmbr_N01",
                table: "edpf_tntmbr",
                newName: "IX_edpf_tntmbr_TenantId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N02",
                table: "edpf_tntdbnd",
                newName: "IX_edpf_tntdbnd_TenantId_Status");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N01",
                table: "edpf_tntdbnd",
                newName: "IX_edpf_tntdbnd_TenantId_Provider");

            migrationBuilder.RenameIndex(
                name: "edpf_tnt_N01",
                table: "edpf_tnt",
                newName: "IX_edpf_tnt_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N02",
                table: "edpf_strqta_plcychg",
                newName: "IX_edpf_strqta_plcychg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N01",
                table: "edpf_strqta_plcychg",
                newName: "IX_edpf_strqta_plcychg_PolicyId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N02",
                table: "edpf_strqta_plcy",
                newName: "IX_edpf_strqta_plcy_TenantId_Scope_TargetId");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N01",
                table: "edpf_strqta_plcy",
                newName: "IX_edpf_strqta_plcy_TenantId_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N03",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N02",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N01",
                table: "edpf_rsrc",
                newName: "IX_edpf_rsrc_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N02",
                table: "edpf_prjmbr",
                newName: "IX_edpf_prjmbr_TenantId_OrganizationId_ProjectId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N01",
                table: "edpf_prjmbr",
                newName: "IX_edpf_prjmbr_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N03",
                table: "edpf_prj",
                newName: "IX_edpf_prj_TenantId_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N02",
                table: "edpf_prj",
                newName: "IX_edpf_prj_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N01",
                table: "edpf_prj",
                newName: "IX_edpf_prj_OrganizationId_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N04",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_project_id_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N03",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_organization_id_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N02",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_Key_Version");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N01",
                table: "edpf_plcy",
                newName: "IX_edpf_plcy_tenant_id");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N02",
                table: "edpf_orgmbr",
                newName: "IX_edpf_orgmbr_TenantId_OrganizationId_SubjectId");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N01",
                table: "edpf_orgmbr",
                newName: "IX_edpf_orgmbr_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N02",
                table: "edpf_org",
                newName: "IX_edpf_org_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N01",
                table: "edpf_org",
                newName: "IX_edpf_org_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N04",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N03",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TenantId_UserId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N02",
                table: "edpf_inv",
                newName: "IX_edpf_inv_UserId");

            migrationBuilder.RenameIndex(
                name: "edpf_inv_N01",
                table: "edpf_inv",
                newName: "IX_edpf_inv_TokenHash");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N02",
                table: "edpf_authplcy_chg",
                newName: "IX_edpf_authplcy_chg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N01",
                table: "edpf_authplcy_chg",
                newName: "IX_edpf_authplcy_chg_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N03",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_TenantId_SubjectId_Action_ResourceType_IsEnabled");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N02",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_ProjectId_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N01",
                table: "edpf_authplcy",
                newName: "IX_edpf_authplcy_OrganizationId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtnchg_N01",
                table: "edpf_authaud_rtnchg",
                newName: "IX_edpf_authaud_rtnchg_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtn_N01",
                table: "edpf_authaud_rtn",
                newName: "IX_edpf_authaud_rtn_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N02",
                table: "edpf_authaud_arcschd",
                newName: "IX_edpf_authaud_arcschd_IsEnabled_NextRunAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N01",
                table: "edpf_authaud_arcschd",
                newName: "IX_edpf_authaud_arcschd_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcjob_N01",
                table: "edpf_authaud_arcjob",
                newName: "IX_edpf_authaud_arcjob_TenantId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N02",
                table: "edpf_authaud_arcattm",
                newName: "IX_edpf_authaud_arcattm_TenantId_JobId_StartedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N01",
                table: "edpf_authaud_arcattm",
                newName: "IX_edpf_authaud_arcattm_JobId_TenantId");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N02",
                table: "edpf_authaud",
                newName: "IX_edpf_authaud_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N01",
                table: "edpf_authaud",
                newName: "IX_edpf_authaud_SubjectId_OccurredAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_usr",
                table: "edpf_usr",
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
                name: "PK_edpf_tntmbr",
                table: "edpf_tntmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tntdbnd",
                table: "edpf_tntdbnd",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_tnt",
                table: "edpf_tnt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_strqta_plcychg",
                table: "edpf_strqta_plcychg",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_strqta_plcy",
                table: "edpf_strqta_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_rsrc",
                table: "edpf_rsrc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_prjmbr",
                table: "edpf_prjmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_prj",
                table: "edpf_prj",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_plcy",
                table: "edpf_plcy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_orgmbr",
                table: "edpf_orgmbr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edpf_org",
                table: "edpf_org",
                column: "Id");

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
    }
}

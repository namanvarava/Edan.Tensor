using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ApplySequentialFrameworkTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Older SQL Server installations used compact legacy column names. Normalize
            // the known tenant-membership key before EF executes the generated rename.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.edpf_tntmbr') IS NOT NULL
   AND COL_LENGTH(N'dbo.edpf_tntmbr', N'tntmbrid') IS NOT NULL
   AND COL_LENGTH(N'dbo.edpf_tntmbr', N'tnt_mbrid') IS NULL
    EXEC sp_rename N'dbo.edpf_tntmbr.tntmbrid', N'tnt_mbrid', N'COLUMN';
IF OBJECT_ID(N'dbo.edw_01002') IS NOT NULL
   AND COL_LENGTH(N'dbo.edw_01002', N'tntmbrid') IS NOT NULL
   AND COL_LENGTH(N'dbo.edw_01002', N'tnt_mbrid') IS NULL
    EXEC sp_rename N'dbo.edw_01002.tntmbrid', N'tnt_mbrid', N'COLUMN';");

            // Existing installations may have foreign-key names or shapes that differ from
            // the generated model. Drop the current platform-schema FKs first; this migration
            // recreates the canonical FKs after the table/PK/column renames.
            migrationBuilder.Sql(@"
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = N'dbo';
IF @sql <> N'' EXEC sp_executesql @sql;");

            migrationBuilder.Sql(@"
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))
    + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';' + CHAR(13) + CHAR(10)
FROM sys.key_constraints
WHERE type = N'UQ' AND OBJECT_SCHEMA_NAME(parent_object_id) = N'dbo';
IF @sql <> N'' EXEC sp_executesql @sql;");





























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

            migrationBuilder.RenameTable(
                name: "edpf_usr",
                newName: "edw_01004");

            migrationBuilder.RenameTable(
                name: "edpf_tntrtnalw",
                newName: "edw_04002");

            migrationBuilder.RenameTable(
                name: "edpf_tntmut_aud",
                newName: "edw_04003");

            migrationBuilder.RenameTable(
                name: "edpf_tntmbr",
                newName: "edw_01002");

            migrationBuilder.RenameTable(
                name: "edpf_tntdbnd",
                newName: "edw_04001");

            migrationBuilder.RenameTable(
                name: "edpf_tnt",
                newName: "edw_01001");

            migrationBuilder.RenameTable(
                name: "edpf_strqta_plcychg",
                newName: "edw_02002");

            migrationBuilder.RenameTable(
                name: "edpf_strqta_plcy",
                newName: "edw_02001");

            migrationBuilder.RenameTable(
                name: "edpf_rsrc",
                newName: "edw_01008");

            migrationBuilder.RenameTable(
                name: "edpf_prjmbr",
                newName: "edw_01006");

            migrationBuilder.RenameTable(
                name: "edpf_prj",
                newName: "edw_01007");

            migrationBuilder.RenameTable(
                name: "edpf_plcy",
                newName: "edw_01009");

            migrationBuilder.RenameTable(
                name: "edpf_orgmbr",
                newName: "edw_01005");

            migrationBuilder.RenameTable(
                name: "edpf_org",
                newName: "edw_01003");

            migrationBuilder.RenameTable(
                name: "edpf_authplcy_chg",
                newName: "edw_01012");

            migrationBuilder.RenameTable(
                name: "edpf_authplcy",
                newName: "edw_01010");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_rtnchg",
                newName: "edw_01014");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_rtn",
                newName: "edw_01013");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcschd",
                newName: "edw_03003");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcjob",
                newName: "edw_03001");

            migrationBuilder.RenameTable(
                name: "edpf_authaud_arcattm",
                newName: "edw_03002");

            migrationBuilder.RenameTable(
                name: "edpf_authaud",
                newName: "edw_01011");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edw_01004",
                newName: "01004id");

            migrationBuilder.RenameIndex(
                name: "edpf_usr_N01",
                table: "edw_01004",
                newName: "edw_01004_N01");

            migrationBuilder.RenameColumn(
                name: "tnt_rtn_alwid",
                table: "edw_04002",
                newName: "04002id");

            migrationBuilder.RenameIndex(
                name: "edpf_tntrtnalw_N01",
                table: "edw_04002",
                newName: "edw_04002_N01");

            migrationBuilder.RenameColumn(
                name: "tnt_mut_audid",
                table: "edw_04003",
                newName: "04003id");

            migrationBuilder.RenameIndex(
                name: "edpf_tntmut_aud_N01",
                table: "edw_04003",
                newName: "edw_04003_N01");

            migrationBuilder.RenameColumn(
                name: "tnt_mbrid",
                table: "edw_01002",
                newName: "01002id");

            migrationBuilder.RenameIndex(
                name: "edpf_tntmbr_N01",
                table: "edw_01002",
                newName: "edw_01002_N01");

            migrationBuilder.RenameColumn(
                name: "tnt_dbndid",
                table: "edw_04001",
                newName: "04001id");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N02",
                table: "edw_04001",
                newName: "edw_04001_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_tntdbnd_N01",
                table: "edw_04001",
                newName: "edw_04001_N01");

            migrationBuilder.RenameColumn(
                name: "tntnm",
                table: "edw_01001",
                newName: "01001nm");

            migrationBuilder.RenameColumn(
                name: "tntcd",
                table: "edw_01001",
                newName: "01001cd");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edw_01001",
                newName: "01001id");

            migrationBuilder.RenameIndex(
                name: "edpf_tnt_N01",
                table: "edw_01001",
                newName: "edw_01001_N01");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcy_chgid",
                table: "edw_02002",
                newName: "02002id");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N02",
                table: "edw_02002",
                newName: "edw_02002_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcychg_N01",
                table: "edw_02002",
                newName: "edw_02002_N01");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcyid",
                table: "edw_02001",
                newName: "02001id");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N02",
                table: "edw_02001",
                newName: "edw_02001_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_strqta_plcy_N01",
                table: "edw_02001",
                newName: "edw_02001_N01");

            migrationBuilder.RenameColumn(
                name: "rsrcnm",
                table: "edw_01008",
                newName: "01008nm");

            migrationBuilder.RenameColumn(
                name: "rsrcid",
                table: "edw_01008",
                newName: "01008id");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N03",
                table: "edw_01008",
                newName: "edw_01008_N03");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N02",
                table: "edw_01008",
                newName: "edw_01008_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_rsrc_N01",
                table: "edw_01008",
                newName: "edw_01008_N01");

            migrationBuilder.RenameColumn(
                name: "prj_mbrid",
                table: "edw_01006",
                newName: "01006id");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N02",
                table: "edw_01006",
                newName: "edw_01006_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_prjmbr_N01",
                table: "edw_01006",
                newName: "edw_01006_N01");

            migrationBuilder.RenameColumn(
                name: "prjnm",
                table: "edw_01007",
                newName: "01007nm");

            migrationBuilder.RenameColumn(
                name: "prjcd",
                table: "edw_01007",
                newName: "01007cd");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edw_01007",
                newName: "01007id");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N03",
                table: "edw_01007",
                newName: "edw_01007_N03");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N02",
                table: "edw_01007",
                newName: "edw_01007_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_prj_N01",
                table: "edw_01007",
                newName: "edw_01007_N01");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edw_01009",
                newName: "01009id");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N04",
                table: "edw_01009",
                newName: "edw_01009_N04");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N03",
                table: "edw_01009",
                newName: "edw_01009_N03");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N02",
                table: "edw_01009",
                newName: "edw_01009_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_plcy_N01",
                table: "edw_01009",
                newName: "edw_01009_N01");

            migrationBuilder.RenameColumn(
                name: "org_mbrid",
                table: "edw_01005",
                newName: "01005id");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N02",
                table: "edw_01005",
                newName: "edw_01005_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_orgmbr_N01",
                table: "edw_01005",
                newName: "edw_01005_N01");

            migrationBuilder.RenameColumn(
                name: "orgnm",
                table: "edw_01003",
                newName: "01003nm");

            migrationBuilder.RenameColumn(
                name: "orgcd",
                table: "edw_01003",
                newName: "01003cd");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edw_01003",
                newName: "01003id");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N02",
                table: "edw_01003",
                newName: "edw_01003_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_org_N01",
                table: "edw_01003",
                newName: "edw_01003_N01");

            migrationBuilder.RenameColumn(
                name: "auth_plcy_chgid",
                table: "edw_01012",
                newName: "01012id");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N02",
                table: "edw_01012",
                newName: "edw_01012_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_chg_N01",
                table: "edw_01012",
                newName: "edw_01012_N01");

            migrationBuilder.RenameColumn(
                name: "auth_plcyid",
                table: "edw_01010",
                newName: "01010id");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N03",
                table: "edw_01010",
                newName: "edw_01010_N03");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N02",
                table: "edw_01010",
                newName: "edw_01010_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_authplcy_N01",
                table: "edw_01010",
                newName: "edw_01010_N01");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtn_chgid",
                table: "edw_01014",
                newName: "01014id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtnchg_N01",
                table: "edw_01014",
                newName: "edw_01014_N01");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtnid",
                table: "edw_01013",
                newName: "01013id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_rtn_N01",
                table: "edw_01013",
                newName: "edw_01013_N01");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_schdid",
                table: "edw_03003",
                newName: "03003id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N02",
                table: "edw_03003",
                newName: "edw_03003_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcschd_N01",
                table: "edw_03003",
                newName: "edw_03003_N01");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_jobid",
                table: "edw_03001",
                newName: "03001id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcjob_N01",
                table: "edw_03001",
                newName: "edw_03001_N01");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_attmid",
                table: "edw_03002",
                newName: "03002id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N02",
                table: "edw_03002",
                newName: "edw_03002_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_arcattm_N01",
                table: "edw_03002",
                newName: "edw_03002_N01");

            migrationBuilder.RenameColumn(
                name: "auth_audid",
                table: "edw_01011",
                newName: "01011id");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N02",
                table: "edw_01011",
                newName: "edw_01011_N02");

            migrationBuilder.RenameIndex(
                name: "edpf_authaud_N01",
                table: "edw_01011",
                newName: "edw_01011_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01004_PK",
                table: "edw_01004",
                column: "01004id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_04002_PK",
                table: "edw_04002",
                column: "04002id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_04003_PK",
                table: "edw_04003",
                column: "04003id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01002_PK",
                table: "edw_01002",
                column: "01002id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_04001_PK",
                table: "edw_04001",
                column: "04001id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01001_PK",
                table: "edw_01001",
                column: "01001id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_02002_PK",
                table: "edw_02002",
                column: "02002id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_02001_02001id_tntid",
                table: "edw_02001",
                columns: new[] { "02001id", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_02001_PK",
                table: "edw_02001",
                column: "02001id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01008_PK",
                table: "edw_01008",
                column: "01008id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01006_PK",
                table: "edw_01006",
                column: "01006id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01007_01007id_orgid_tntid",
                table: "edw_01007",
                columns: new[] { "01007id", "orgid", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_01007_PK",
                table: "edw_01007",
                column: "01007id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01009_PK",
                table: "edw_01009",
                column: "01009id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01005_PK",
                table: "edw_01005",
                column: "01005id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01003_01003id_tntid",
                table: "edw_01003",
                columns: new[] { "01003id", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_01003_PK",
                table: "edw_01003",
                column: "01003id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01012_PK",
                table: "edw_01012",
                column: "01012id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01010_PK",
                table: "edw_01010",
                column: "01010id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01014_PK",
                table: "edw_01014",
                column: "01014id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01013_PK",
                table: "edw_01013",
                column: "01013id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03003_PK",
                table: "edw_03003",
                column: "03003id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_03001_03001id_tntid",
                table: "edw_03001",
                columns: new[] { "03001id", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_03001_PK",
                table: "edw_03001",
                column: "03001id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03002_PK",
                table: "edw_03002",
                column: "03002id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_01011_PK",
                table: "edw_01011",
                column: "01011id");

            migrationBuilder.AddForeignKey(
                name: "edw_01002_FKTenantId",
                table: "edw_01002",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01003_FKTenantId",
                table: "edw_01003",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01004_FKTenantId",
                table: "edw_01004",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01005_FKOrganizationId",
                table: "edw_01005",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edw_01003",
                principalColumns: new[] { "01003id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01006_FKProjectId",
                table: "edw_01006",
                columns: new[] { "prjid", "orgid", "tntid" },
                principalTable: "edw_01007",
                principalColumns: new[] { "01007id", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01007_FKOrganizationId",
                table: "edw_01007",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edw_01003",
                principalColumns: new[] { "01003id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01007_FKTenantId",
                table: "edw_01007",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01008_FKScopeOrganizationId",
                table: "edw_01008",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edw_01003",
                principalColumns: new[] { "01003id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01008_FKScopeProjectId",
                table: "edw_01008",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edw_01007",
                principalColumns: new[] { "01007id", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01008_FKScopeTenantId",
                table: "edw_01008",
                column: "tenant_id",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01009_FKScopeOrganizationId",
                table: "edw_01009",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edw_01003",
                principalColumns: new[] { "01003id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01009_FKScopeProjectId",
                table: "edw_01009",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edw_01007",
                principalColumns: new[] { "01007id", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01009_FKScopeTenantId",
                table: "edw_01009",
                column: "tenant_id",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01010_FKOrganizationId",
                table: "edw_01010",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edw_01003",
                principalColumns: new[] { "01003id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01010_FKProjectId",
                table: "edw_01010",
                columns: new[] { "prjid", "orgid", "tntid" },
                principalTable: "edw_01007",
                principalColumns: new[] { "01007id", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01010_FKTenantId",
                table: "edw_01010",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01011_FKTenantId",
                table: "edw_01011",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01012_FKTenantId",
                table: "edw_01012",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01013_FKTenantId",
                table: "edw_01013",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_01014_FKTenantId",
                table: "edw_01014",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_02001_FKTenantId",
                table: "edw_02001",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_02002_FKPolicyId",
                table: "edw_02002",
                columns: new[] { "plcyid", "tntid" },
                principalTable: "edw_02001",
                principalColumns: new[] { "02001id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_02002_FKTenantId",
                table: "edw_02002",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_03001_FKTenantId",
                table: "edw_03001",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_03002_FKJobId",
                table: "edw_03002",
                columns: new[] { "jobid", "tntid" },
                principalTable: "edw_03001",
                principalColumns: new[] { "03001id", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_03002_FKTenantId",
                table: "edw_03002",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_03003_FKTenantId",
                table: "edw_03003",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_04001_FKTenantId",
                table: "edw_04001",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_04002_FKTenantId",
                table: "edw_04002",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edw_04003_FKTenantId",
                table: "edw_04003",
                column: "tntid",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {





























migrationBuilder.DropPrimaryKey(
                name: "edw_04003_PK",
                table: "edw_04003");

            migrationBuilder.DropPrimaryKey(
                name: "edw_04002_PK",
                table: "edw_04002");

            migrationBuilder.DropPrimaryKey(
                name: "edw_04001_PK",
                table: "edw_04001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03003_PK",
                table: "edw_03003");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03002_PK",
                table: "edw_03002");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_03001_03001id_tntid",
                table: "edw_03001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03001_PK",
                table: "edw_03001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_02002_PK",
                table: "edw_02002");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_02001_02001id_tntid",
                table: "edw_02001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_02001_PK",
                table: "edw_02001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01014_PK",
                table: "edw_01014");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01013_PK",
                table: "edw_01013");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01012_PK",
                table: "edw_01012");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01011_PK",
                table: "edw_01011");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01010_PK",
                table: "edw_01010");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01009_PK",
                table: "edw_01009");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01008_PK",
                table: "edw_01008");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_01007_01007id_orgid_tntid",
                table: "edw_01007");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01007_PK",
                table: "edw_01007");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01006_PK",
                table: "edw_01006");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01005_PK",
                table: "edw_01005");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01004_PK",
                table: "edw_01004");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_01003_01003id_tntid",
                table: "edw_01003");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01003_PK",
                table: "edw_01003");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01002_PK",
                table: "edw_01002");

            migrationBuilder.DropPrimaryKey(
                name: "edw_01001_PK",
                table: "edw_01001");

            migrationBuilder.RenameTable(
                name: "edw_04003",
                newName: "edpf_tntmut_aud");

            migrationBuilder.RenameTable(
                name: "edw_04002",
                newName: "edpf_tntrtnalw");

            migrationBuilder.RenameTable(
                name: "edw_04001",
                newName: "edpf_tntdbnd");

            migrationBuilder.RenameTable(
                name: "edw_03003",
                newName: "edpf_authaud_arcschd");

            migrationBuilder.RenameTable(
                name: "edw_03002",
                newName: "edpf_authaud_arcattm");

            migrationBuilder.RenameTable(
                name: "edw_03001",
                newName: "edpf_authaud_arcjob");

            migrationBuilder.RenameTable(
                name: "edw_02002",
                newName: "edpf_strqta_plcychg");

            migrationBuilder.RenameTable(
                name: "edw_02001",
                newName: "edpf_strqta_plcy");

            migrationBuilder.RenameTable(
                name: "edw_01014",
                newName: "edpf_authaud_rtnchg");

            migrationBuilder.RenameTable(
                name: "edw_01013",
                newName: "edpf_authaud_rtn");

            migrationBuilder.RenameTable(
                name: "edw_01012",
                newName: "edpf_authplcy_chg");

            migrationBuilder.RenameTable(
                name: "edw_01011",
                newName: "edpf_authaud");

            migrationBuilder.RenameTable(
                name: "edw_01010",
                newName: "edpf_authplcy");

            migrationBuilder.RenameTable(
                name: "edw_01009",
                newName: "edpf_plcy");

            migrationBuilder.RenameTable(
                name: "edw_01008",
                newName: "edpf_rsrc");

            migrationBuilder.RenameTable(
                name: "edw_01007",
                newName: "edpf_prj");

            migrationBuilder.RenameTable(
                name: "edw_01006",
                newName: "edpf_prjmbr");

            migrationBuilder.RenameTable(
                name: "edw_01005",
                newName: "edpf_orgmbr");

            migrationBuilder.RenameTable(
                name: "edw_01004",
                newName: "edpf_usr");

            migrationBuilder.RenameTable(
                name: "edw_01003",
                newName: "edpf_org");

            migrationBuilder.RenameTable(
                name: "edw_01002",
                newName: "edpf_tntmbr");

            migrationBuilder.RenameTable(
                name: "edw_01001",
                newName: "edpf_tnt");

            migrationBuilder.RenameColumn(
                name: "04003id",
                table: "edpf_tntmut_aud",
                newName: "tnt_mut_audid");

            migrationBuilder.RenameIndex(
                name: "edw_04003_N01",
                table: "edpf_tntmut_aud",
                newName: "edpf_tntmut_aud_N01");

            migrationBuilder.RenameColumn(
                name: "04002id",
                table: "edpf_tntrtnalw",
                newName: "tnt_rtn_alwid");

            migrationBuilder.RenameIndex(
                name: "edw_04002_N01",
                table: "edpf_tntrtnalw",
                newName: "edpf_tntrtnalw_N01");

            migrationBuilder.RenameColumn(
                name: "04001id",
                table: "edpf_tntdbnd",
                newName: "tnt_dbndid");

            migrationBuilder.RenameIndex(
                name: "edw_04001_N02",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N02");

            migrationBuilder.RenameIndex(
                name: "edw_04001_N01",
                table: "edpf_tntdbnd",
                newName: "edpf_tntdbnd_N01");

            migrationBuilder.RenameColumn(
                name: "03003id",
                table: "edpf_authaud_arcschd",
                newName: "auth_aud_arc_schdid");

            migrationBuilder.RenameIndex(
                name: "edw_03003_N02",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N02");

            migrationBuilder.RenameIndex(
                name: "edw_03003_N01",
                table: "edpf_authaud_arcschd",
                newName: "edpf_authaud_arcschd_N01");

            migrationBuilder.RenameColumn(
                name: "03002id",
                table: "edpf_authaud_arcattm",
                newName: "auth_aud_arc_attmid");

            migrationBuilder.RenameIndex(
                name: "edw_03002_N02",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N02");

            migrationBuilder.RenameIndex(
                name: "edw_03002_N01",
                table: "edpf_authaud_arcattm",
                newName: "edpf_authaud_arcattm_N01");

            migrationBuilder.RenameColumn(
                name: "03001id",
                table: "edpf_authaud_arcjob",
                newName: "auth_aud_arc_jobid");

            migrationBuilder.RenameIndex(
                name: "edw_03001_N01",
                table: "edpf_authaud_arcjob",
                newName: "edpf_authaud_arcjob_N01");

            migrationBuilder.RenameColumn(
                name: "02002id",
                table: "edpf_strqta_plcychg",
                newName: "str_qta_plcy_chgid");

            migrationBuilder.RenameIndex(
                name: "edw_02002_N02",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N02");

            migrationBuilder.RenameIndex(
                name: "edw_02002_N01",
                table: "edpf_strqta_plcychg",
                newName: "edpf_strqta_plcychg_N01");

            migrationBuilder.RenameColumn(
                name: "02001id",
                table: "edpf_strqta_plcy",
                newName: "str_qta_plcyid");

            migrationBuilder.RenameIndex(
                name: "edw_02001_N02",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "edw_02001_N01",
                table: "edpf_strqta_plcy",
                newName: "edpf_strqta_plcy_N01");

            migrationBuilder.RenameColumn(
                name: "01014id",
                table: "edpf_authaud_rtnchg",
                newName: "auth_aud_rtn_chgid");

            migrationBuilder.RenameIndex(
                name: "edw_01014_N01",
                table: "edpf_authaud_rtnchg",
                newName: "edpf_authaud_rtnchg_N01");

            migrationBuilder.RenameColumn(
                name: "01013id",
                table: "edpf_authaud_rtn",
                newName: "auth_aud_rtnid");

            migrationBuilder.RenameIndex(
                name: "edw_01013_N01",
                table: "edpf_authaud_rtn",
                newName: "edpf_authaud_rtn_N01");

            migrationBuilder.RenameColumn(
                name: "01012id",
                table: "edpf_authplcy_chg",
                newName: "auth_plcy_chgid");

            migrationBuilder.RenameIndex(
                name: "edw_01012_N02",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01012_N01",
                table: "edpf_authplcy_chg",
                newName: "edpf_authplcy_chg_N01");

            migrationBuilder.RenameColumn(
                name: "01011id",
                table: "edpf_authaud",
                newName: "auth_audid");

            migrationBuilder.RenameIndex(
                name: "edw_01011_N02",
                table: "edpf_authaud",
                newName: "edpf_authaud_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01011_N01",
                table: "edpf_authaud",
                newName: "edpf_authaud_N01");

            migrationBuilder.RenameColumn(
                name: "01010id",
                table: "edpf_authplcy",
                newName: "auth_plcyid");

            migrationBuilder.RenameIndex(
                name: "edw_01010_N03",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N03");

            migrationBuilder.RenameIndex(
                name: "edw_01010_N02",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01010_N01",
                table: "edpf_authplcy",
                newName: "edpf_authplcy_N01");

            migrationBuilder.RenameColumn(
                name: "01009id",
                table: "edpf_plcy",
                newName: "plcyid");

            migrationBuilder.RenameIndex(
                name: "edw_01009_N04",
                table: "edpf_plcy",
                newName: "edpf_plcy_N04");

            migrationBuilder.RenameIndex(
                name: "edw_01009_N03",
                table: "edpf_plcy",
                newName: "edpf_plcy_N03");

            migrationBuilder.RenameIndex(
                name: "edw_01009_N02",
                table: "edpf_plcy",
                newName: "edpf_plcy_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01009_N01",
                table: "edpf_plcy",
                newName: "edpf_plcy_N01");

            migrationBuilder.RenameColumn(
                name: "01008nm",
                table: "edpf_rsrc",
                newName: "rsrcnm");

            migrationBuilder.RenameColumn(
                name: "01008id",
                table: "edpf_rsrc",
                newName: "rsrcid");

            migrationBuilder.RenameIndex(
                name: "edw_01008_N03",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N03");

            migrationBuilder.RenameIndex(
                name: "edw_01008_N02",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01008_N01",
                table: "edpf_rsrc",
                newName: "edpf_rsrc_N01");

            migrationBuilder.RenameColumn(
                name: "01007nm",
                table: "edpf_prj",
                newName: "prjnm");

            migrationBuilder.RenameColumn(
                name: "01007cd",
                table: "edpf_prj",
                newName: "prjcd");

            migrationBuilder.RenameColumn(
                name: "01007id",
                table: "edpf_prj",
                newName: "prjid");

            migrationBuilder.RenameIndex(
                name: "edw_01007_N03",
                table: "edpf_prj",
                newName: "edpf_prj_N03");

            migrationBuilder.RenameIndex(
                name: "edw_01007_N02",
                table: "edpf_prj",
                newName: "edpf_prj_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01007_N01",
                table: "edpf_prj",
                newName: "edpf_prj_N01");

            migrationBuilder.RenameColumn(
                name: "01006id",
                table: "edpf_prjmbr",
                newName: "prj_mbrid");

            migrationBuilder.RenameIndex(
                name: "edw_01006_N02",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01006_N01",
                table: "edpf_prjmbr",
                newName: "edpf_prjmbr_N01");

            migrationBuilder.RenameColumn(
                name: "01005id",
                table: "edpf_orgmbr",
                newName: "org_mbrid");

            migrationBuilder.RenameIndex(
                name: "edw_01005_N02",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01005_N01",
                table: "edpf_orgmbr",
                newName: "edpf_orgmbr_N01");

            migrationBuilder.RenameColumn(
                name: "01004id",
                table: "edpf_usr",
                newName: "usrid");

            migrationBuilder.RenameIndex(
                name: "edw_01004_N01",
                table: "edpf_usr",
                newName: "edpf_usr_N01");

            migrationBuilder.RenameColumn(
                name: "01003nm",
                table: "edpf_org",
                newName: "orgnm");

            migrationBuilder.RenameColumn(
                name: "01003cd",
                table: "edpf_org",
                newName: "orgcd");

            migrationBuilder.RenameColumn(
                name: "01003id",
                table: "edpf_org",
                newName: "orgid");

            migrationBuilder.RenameIndex(
                name: "edw_01003_N02",
                table: "edpf_org",
                newName: "edpf_org_N02");

            migrationBuilder.RenameIndex(
                name: "edw_01003_N01",
                table: "edpf_org",
                newName: "edpf_org_N01");

            migrationBuilder.RenameColumn(
                name: "01002id",
                table: "edpf_tntmbr",
                newName: "tnt_mbrid");

            migrationBuilder.RenameIndex(
                name: "edw_01002_N01",
                table: "edpf_tntmbr",
                newName: "edpf_tntmbr_N01");

            migrationBuilder.RenameColumn(
                name: "01001nm",
                table: "edpf_tnt",
                newName: "tntnm");

            migrationBuilder.RenameColumn(
                name: "01001cd",
                table: "edpf_tnt",
                newName: "tntcd");

            migrationBuilder.RenameColumn(
                name: "01001id",
                table: "edpf_tnt",
                newName: "tntid");

            migrationBuilder.RenameIndex(
                name: "edw_01001_N01",
                table: "edpf_tnt",
                newName: "edpf_tnt_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmut_aud_PK",
                table: "edpf_tntmut_aud",
                column: "tnt_mut_audid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntrtnalw_PK",
                table: "edpf_tntrtnalw",
                column: "tnt_rtn_alwid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntdbnd_PK",
                table: "edpf_tntdbnd",
                column: "tnt_dbndid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcschd_PK",
                table: "edpf_authaud_arcschd",
                column: "auth_aud_arc_schdid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcattm_PK",
                table: "edpf_authaud_arcattm",
                column: "auth_aud_arc_attmid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid",
                table: "edpf_authaud_arcjob",
                columns: new[] { "auth_aud_arc_jobid", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_arcjob_PK",
                table: "edpf_authaud_arcjob",
                column: "auth_aud_arc_jobid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcychg_PK",
                table: "edpf_strqta_plcychg",
                column: "str_qta_plcy_chgid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_str_qta_plcyid_tntid",
                table: "edpf_strqta_plcy",
                columns: new[] { "str_qta_plcyid", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy",
                column: "str_qta_plcyid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_rtnchg_PK",
                table: "edpf_authaud_rtnchg",
                column: "auth_aud_rtn_chgid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_rtn_PK",
                table: "edpf_authaud_rtn",
                column: "auth_aud_rtnid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authplcy_chg_PK",
                table: "edpf_authplcy_chg",
                column: "auth_plcy_chgid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authaud_PK",
                table: "edpf_authaud",
                column: "auth_audid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_authplcy_PK",
                table: "edpf_authplcy",
                column: "auth_plcyid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy",
                column: "plcyid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc",
                column: "rsrcid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_prjid_orgid_tntid",
                table: "edpf_prj",
                columns: new[] { "prjid", "orgid", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj",
                column: "prjid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr",
                column: "prj_mbrid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr",
                column: "org_mbrid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_usr_PK",
                table: "edpf_usr",
                column: "usrid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_orgid_tntid",
                table: "edpf_org",
                columns: new[] { "orgid", "tntid" });

            migrationBuilder.AddPrimaryKey(
                name: "edpf_org_PK",
                table: "edpf_org",
                column: "orgid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tntmbr_PK",
                table: "edpf_tntmbr",
                column: "tnt_mbrid");

            migrationBuilder.AddPrimaryKey(
                name: "edpf_tnt_PK",
                table: "edpf_tnt",
                column: "tntid");

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_FKTenantId",
                table: "edpf_authaud",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcattm_FKJobId",
                table: "edpf_authaud_arcattm",
                columns: new[] { "jobid", "tntid" },
                principalTable: "edpf_authaud_arcjob",
                principalColumns: new[] { "auth_aud_arc_jobid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcattm_FKTenantId",
                table: "edpf_authaud_arcattm",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcjob_FKTenantId",
                table: "edpf_authaud_arcjob",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_arcschd_FKTenantId",
                table: "edpf_authaud_arcschd",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_rtn_FKTenantId",
                table: "edpf_authaud_rtn",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authaud_rtnchg_FKTenantId",
                table: "edpf_authaud_rtnchg",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKOrganizationId",
                table: "edpf_authplcy",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edpf_org",
                principalColumns: new[] { "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKProjectId",
                table: "edpf_authplcy",
                columns: new[] { "prjid", "orgid", "tntid" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "prjid", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_FKTenantId",
                table: "edpf_authplcy",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_authplcy_chg_FKTenantId",
                table: "edpf_authplcy_chg",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_org_FKTenantId",
                table: "edpf_org",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_orgmbr_FKOrganizationId",
                table: "edpf_orgmbr",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edpf_org",
                principalColumns: new[] { "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeOrganizationId",
                table: "edpf_plcy",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeProjectId",
                table: "edpf_plcy",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "prjid", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_plcy_FKScopeTenantId",
                table: "edpf_plcy",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prj_FKOrganizationId",
                table: "edpf_prj",
                columns: new[] { "orgid", "tntid" },
                principalTable: "edpf_org",
                principalColumns: new[] { "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prj_FKTenantId",
                table: "edpf_prj",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_prjmbr_FKProjectId",
                table: "edpf_prjmbr",
                columns: new[] { "prjid", "orgid", "tntid" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "prjid", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeOrganizationId",
                table: "edpf_rsrc",
                columns: new[] { "organization_id", "tenant_id" },
                principalTable: "edpf_org",
                principalColumns: new[] { "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeProjectId",
                table: "edpf_rsrc",
                columns: new[] { "project_id", "organization_id", "tenant_id" },
                principalTable: "edpf_prj",
                principalColumns: new[] { "prjid", "orgid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_rsrc_FKScopeTenantId",
                table: "edpf_rsrc",
                column: "tenant_id",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcy_FKTenantId",
                table: "edpf_strqta_plcy",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcychg_FKPolicyId",
                table: "edpf_strqta_plcychg",
                columns: new[] { "plcyid", "tntid" },
                principalTable: "edpf_strqta_plcy",
                principalColumns: new[] { "str_qta_plcyid", "tntid" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_strqta_plcychg_FKTenantId",
                table: "edpf_strqta_plcychg",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntdbnd_FKTenantId",
                table: "edpf_tntdbnd",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntmbr_FKTenantId",
                table: "edpf_tntmbr",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntmut_aud_FKTenantId",
                table: "edpf_tntmut_aud",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_tntrtnalw_FKTenantId",
                table: "edpf_tntrtnalw",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "edpf_usr_FKTenantId",
                table: "edpf_usr",
                column: "tntid",
                principalTable: "edpf_tnt",
                principalColumn: "tntid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

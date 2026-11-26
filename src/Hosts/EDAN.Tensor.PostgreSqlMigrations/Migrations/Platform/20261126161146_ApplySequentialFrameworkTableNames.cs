using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ApplySequentialFrameworkTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing installations may already have semantic constraint names from the prior schema migration.
            // Normalize those names before EF drops/recreates them so this rename migration is repeatable.
            migrationBuilder.Sql("""
DO $$
DECLARE
    item record;
    existing_name text;
BEGIN
    FOR item IN
        SELECT * FROM (VALUES
                ('edpf_authaud','edpf_authaud_FKTenantId','f'),
                ('edpf_authaud_arcattm','edpf_authaud_arcattm_FKJobId','f'),
                ('edpf_authaud_arcattm','edpf_authaud_arcattm_FKTenantId','f'),
                ('edpf_authaud_arcjob','edpf_authaud_arcjob_FKTenantId','f'),
                ('edpf_authaud_arcschd','edpf_authaud_arcschd_FKTenantId','f'),
                ('edpf_authaud_rtn','edpf_authaud_rtn_FKTenantId','f'),
                ('edpf_authaud_rtnchg','edpf_authaud_rtnchg_FKTenantId','f'),
                ('edpf_authplcy','edpf_authplcy_FKOrganizationId','f'),
                ('edpf_authplcy','edpf_authplcy_FKProjectId','f'),
                ('edpf_authplcy','edpf_authplcy_FKTenantId','f'),
                ('edpf_authplcy_chg','edpf_authplcy_chg_FKTenantId','f'),
                ('edpf_org','edpf_org_FKTenantId','f'),
                ('edpf_orgmbr','edpf_orgmbr_FKOrganizationId','f'),
                ('edpf_plcy','edpf_plcy_FKScopeOrganizationId','f'),
                ('edpf_plcy','edpf_plcy_FKScopeProjectId','f'),
                ('edpf_plcy','edpf_plcy_FKScopeTenantId','f'),
                ('edpf_prj','edpf_prj_FKOrganizationId','f'),
                ('edpf_prj','edpf_prj_FKTenantId','f'),
                ('edpf_prjmbr','edpf_prjmbr_FKProjectId','f'),
                ('edpf_rsrc','edpf_rsrc_FKScopeOrganizationId','f'),
                ('edpf_rsrc','edpf_rsrc_FKScopeProjectId','f'),
                ('edpf_rsrc','edpf_rsrc_FKScopeTenantId','f'),
                ('edpf_strqta_plcy','edpf_strqta_plcy_FKTenantId','f'),
                ('edpf_strqta_plcychg','edpf_strqta_plcychg_FKPolicyId','f'),
                ('edpf_strqta_plcychg','edpf_strqta_plcychg_FKTenantId','f'),
                ('edpf_tntdbnd','edpf_tntdbnd_FKTenantId','f'),
                ('edpf_tntmbr','edpf_tntmbr_FKTenantId','f'),
                ('edpf_tntmut_aud','edpf_tntmut_aud_FKTenantId','f'),
                ('edpf_tntrtnalw','edpf_tntrtnalw_FKTenantId','f'),
                ('edpf_usr','edpf_usr_FKTenantId','f'),
                ('edw_01002','edw_01002_FKTenantId','f'),
                ('edw_01003','edw_01003_FKTenantId','f'),
                ('edw_01004','edw_01004_FKTenantId','f'),
                ('edw_01005','edw_01005_FKOrganizationId','f'),
                ('edw_01006','edw_01006_FKProjectId','f'),
                ('edw_01007','edw_01007_FKOrganizationId','f'),
                ('edw_01007','edw_01007_FKTenantId','f'),
                ('edw_01008','edw_01008_FKScopeOrganizationId','f'),
                ('edw_01008','edw_01008_FKScopeProjectId','f'),
                ('edw_01008','edw_01008_FKScopeTenantId','f'),
                ('edw_01009','edw_01009_FKScopeOrganizationId','f'),
                ('edw_01009','edw_01009_FKScopeProjectId','f'),
                ('edw_01009','edw_01009_FKScopeTenantId','f'),
                ('edw_01010','edw_01010_FKOrganizationId','f'),
                ('edw_01010','edw_01010_FKProjectId','f'),
                ('edw_01010','edw_01010_FKTenantId','f'),
                ('edw_01011','edw_01011_FKTenantId','f'),
                ('edw_01012','edw_01012_FKTenantId','f'),
                ('edw_01013','edw_01013_FKTenantId','f'),
                ('edw_01014','edw_01014_FKTenantId','f'),
                ('edw_02001','edw_02001_FKTenantId','f'),
                ('edw_02002','edw_02002_FKPolicyId','f'),
                ('edw_02002','edw_02002_FKTenantId','f'),
                ('edw_03001','edw_03001_FKTenantId','f'),
                ('edw_03002','edw_03002_FKJobId','f'),
                ('edw_03002','edw_03002_FKTenantId','f'),
                ('edw_03003','edw_03003_FKTenantId','f'),
                ('edw_04001','edw_04001_FKTenantId','f'),
                ('edw_04002','edw_04002_FKTenantId','f'),
                ('edw_04003','edw_04003_FKTenantId','f'),
                ('edpf_usr','edpf_usr_PK','p'),
                ('edpf_tntrtnalw','edpf_tntrtnalw_PK','p'),
                ('edpf_tntmut_aud','edpf_tntmut_aud_PK','p'),
                ('edpf_tntmbr','edpf_tntmbr_PK','p'),
                ('edpf_tntdbnd','edpf_tntdbnd_PK','p'),
                ('edpf_tnt','edpf_tnt_PK','p'),
                ('edpf_strqta_plcychg','edpf_strqta_plcychg_PK','p'),
                ('edpf_strqta_plcy','edpf_strqta_plcy_PK','p'),
                ('edpf_rsrc','edpf_rsrc_PK','p'),
                ('edpf_prjmbr','edpf_prjmbr_PK','p'),
                ('edpf_prj','edpf_prj_PK','p'),
                ('edpf_plcy','edpf_plcy_PK','p'),
                ('edpf_orgmbr','edpf_orgmbr_PK','p'),
                ('edpf_org','edpf_org_PK','p'),
                ('edpf_authplcy_chg','edpf_authplcy_chg_PK','p'),
                ('edpf_authplcy','edpf_authplcy_PK','p'),
                ('edpf_authaud_rtnchg','edpf_authaud_rtnchg_PK','p'),
                ('edpf_authaud_rtn','edpf_authaud_rtn_PK','p'),
                ('edpf_authaud_arcschd','edpf_authaud_arcschd_PK','p'),
                ('edpf_authaud_arcjob','edpf_authaud_arcjob_PK','p'),
                ('edpf_authaud_arcattm','edpf_authaud_arcattm_PK','p'),
                ('edpf_authaud','edpf_authaud_PK','p'),
                ('edw_04003','edw_04003_PK','p'),
                ('edw_04002','edw_04002_PK','p'),
                ('edw_04001','edw_04001_PK','p'),
                ('edw_03003','edw_03003_PK','p'),
                ('edw_03002','edw_03002_PK','p'),
                ('edw_03001','edw_03001_PK','p'),
                ('edw_02002','edw_02002_PK','p'),
                ('edw_02001','edw_02001_PK','p'),
                ('edw_01014','edw_01014_PK','p'),
                ('edw_01013','edw_01013_PK','p'),
                ('edw_01012','edw_01012_PK','p'),
                ('edw_01011','edw_01011_PK','p'),
                ('edw_01010','edw_01010_PK','p'),
                ('edw_01009','edw_01009_PK','p'),
                ('edw_01008','edw_01008_PK','p'),
                ('edw_01007','edw_01007_PK','p'),
                ('edw_01006','edw_01006_PK','p'),
                ('edw_01005','edw_01005_PK','p'),
                ('edw_01004','edw_01004_PK','p'),
                ('edw_01003','edw_01003_PK','p'),
                ('edw_01002','edw_01002_PK','p'),
                ('edw_01001','edw_01001_PK','p'),
                ('edpf_strqta_plcy','AK_edpf_strqta_plcy_str_qta_plcyid_tntid','u'),
                ('edpf_prj','AK_edpf_prj_prjid_orgid_tntid','u'),
                ('edpf_org','AK_edpf_org_orgid_tntid','u'),
                ('edpf_authaud_arcjob','AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid','u'),
                ('edw_03001','AK_edw_03001_03001id_tntid','u'),
                ('edw_02001','AK_edw_02001_02001id_tntid','u'),
                ('edw_01007','AK_edw_01007_01007id_orgid_tntid','u'),
                ('edw_01003','AK_edw_01003_01003id_tntid','u')
        ) AS v(table_name, target_name, constraint_kind)
    LOOP
        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'public'
              AND t.relname = item.table_name
              AND c.conname = item.target_name
        ) THEN
            SELECT c.conname INTO existing_name
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'public'
              AND t.relname = item.table_name
              AND c.contype = item.constraint_kind
            ORDER BY c.conname
            LIMIT 1;

            IF existing_name IS NOT NULL THEN
                EXECUTE format('ALTER TABLE %I RENAME CONSTRAINT %I TO %I', item.table_name, existing_name, item.target_name);
            END IF;
        END IF;
    END LOOP;
END $$;
""");

            // Invitations are intentionally retained outside the sequential table set,
            // but their references must be detached while the tenant/user PKs are renamed.
            migrationBuilder.DropForeignKey(
                name: "FK_platform_invitations_platform_tenants_TenantId",
                table: "edpf_inv");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_invitations_platform_users_UserId",
                table: "edpf_inv");
            // Normalize pre-existing semantic index names before EF renames them.
            migrationBuilder.Sql("""
DO $$
DECLARE
    item record;
    existing_name text;
    source_table text;
BEGIN
    FOR item IN
        SELECT * FROM (VALUES
                ('edw_01004','edpf_usr_N01','edw_01004_N01'),
                ('edw_04002','edpf_tntrtnalw_N01','edw_04002_N01'),
                ('edw_04003','edpf_tntmut_aud_N01','edw_04003_N01'),
                ('edw_01002','edpf_tntmbr_N01','edw_01002_N01'),
                ('edw_04001','edpf_tntdbnd_N02','edw_04001_N02'),
                ('edw_04001','edpf_tntdbnd_N01','edw_04001_N01'),
                ('edw_01001','edpf_tnt_N01','edw_01001_N01'),
                ('edw_02002','edpf_strqta_plcychg_N02','edw_02002_N02'),
                ('edw_02002','edpf_strqta_plcychg_N01','edw_02002_N01'),
                ('edw_02001','edpf_strqta_plcy_N02','edw_02001_N02'),
                ('edw_02001','edpf_strqta_plcy_N01','edw_02001_N01'),
                ('edw_01008','edpf_rsrc_N03','edw_01008_N03'),
                ('edw_01008','edpf_rsrc_N02','edw_01008_N02'),
                ('edw_01008','edpf_rsrc_N01','edw_01008_N01'),
                ('edw_01006','edpf_prjmbr_N02','edw_01006_N02'),
                ('edw_01006','edpf_prjmbr_N01','edw_01006_N01'),
                ('edw_01007','edpf_prj_N03','edw_01007_N03'),
                ('edw_01007','edpf_prj_N02','edw_01007_N02'),
                ('edw_01007','edpf_prj_N01','edw_01007_N01'),
                ('edw_01009','edpf_plcy_N04','edw_01009_N04'),
                ('edw_01009','edpf_plcy_N03','edw_01009_N03'),
                ('edw_01009','edpf_plcy_N02','edw_01009_N02'),
                ('edw_01009','edpf_plcy_N01','edw_01009_N01'),
                ('edw_01005','edpf_orgmbr_N02','edw_01005_N02'),
                ('edw_01005','edpf_orgmbr_N01','edw_01005_N01'),
                ('edw_01003','edpf_org_N02','edw_01003_N02'),
                ('edw_01003','edpf_org_N01','edw_01003_N01'),
                ('edw_01012','edpf_authplcy_chg_N02','edw_01012_N02'),
                ('edw_01012','edpf_authplcy_chg_N01','edw_01012_N01'),
                ('edw_01010','edpf_authplcy_N03','edw_01010_N03'),
                ('edw_01010','edpf_authplcy_N02','edw_01010_N02'),
                ('edw_01010','edpf_authplcy_N01','edw_01010_N01'),
                ('edw_01014','edpf_authaud_rtnchg_N01','edw_01014_N01'),
                ('edw_01013','edpf_authaud_rtn_N01','edw_01013_N01'),
                ('edw_03003','edpf_authaud_arcschd_N02','edw_03003_N02'),
                ('edw_03003','edpf_authaud_arcschd_N01','edw_03003_N01'),
                ('edw_03001','edpf_authaud_arcjob_N01','edw_03001_N01'),
                ('edw_03002','edpf_authaud_arcattm_N02','edw_03002_N02'),
                ('edw_03002','edpf_authaud_arcattm_N01','edw_03002_N01'),
                ('edw_01011','edpf_authaud_N02','edw_01011_N02'),
                ('edw_01011','edpf_authaud_N01','edw_01011_N01'),
                ('edpf_tntmut_aud','edw_04003_N01','edpf_tntmut_aud_N01'),
                ('edpf_tntrtnalw','edw_04002_N01','edpf_tntrtnalw_N01'),
                ('edpf_tntdbnd','edw_04001_N02','edpf_tntdbnd_N02'),
                ('edpf_tntdbnd','edw_04001_N01','edpf_tntdbnd_N01'),
                ('edpf_authaud_arcschd','edw_03003_N02','edpf_authaud_arcschd_N02'),
                ('edpf_authaud_arcschd','edw_03003_N01','edpf_authaud_arcschd_N01'),
                ('edpf_authaud_arcattm','edw_03002_N02','edpf_authaud_arcattm_N02'),
                ('edpf_authaud_arcattm','edw_03002_N01','edpf_authaud_arcattm_N01'),
                ('edpf_authaud_arcjob','edw_03001_N01','edpf_authaud_arcjob_N01'),
                ('edpf_strqta_plcychg','edw_02002_N02','edpf_strqta_plcychg_N02'),
                ('edpf_strqta_plcychg','edw_02002_N01','edpf_strqta_plcychg_N01'),
                ('edpf_strqta_plcy','edw_02001_N02','edpf_strqta_plcy_N02'),
                ('edpf_strqta_plcy','edw_02001_N01','edpf_strqta_plcy_N01'),
                ('edpf_authaud_rtnchg','edw_01014_N01','edpf_authaud_rtnchg_N01'),
                ('edpf_authaud_rtn','edw_01013_N01','edpf_authaud_rtn_N01'),
                ('edpf_authplcy_chg','edw_01012_N02','edpf_authplcy_chg_N02'),
                ('edpf_authplcy_chg','edw_01012_N01','edpf_authplcy_chg_N01'),
                ('edpf_authaud','edw_01011_N02','edpf_authaud_N02'),
                ('edpf_authaud','edw_01011_N01','edpf_authaud_N01'),
                ('edpf_authplcy','edw_01010_N03','edpf_authplcy_N03'),
                ('edpf_authplcy','edw_01010_N02','edpf_authplcy_N02'),
                ('edpf_authplcy','edw_01010_N01','edpf_authplcy_N01'),
                ('edpf_plcy','edw_01009_N04','edpf_plcy_N04'),
                ('edpf_plcy','edw_01009_N03','edpf_plcy_N03'),
                ('edpf_plcy','edw_01009_N02','edpf_plcy_N02'),
                ('edpf_plcy','edw_01009_N01','edpf_plcy_N01'),
                ('edpf_rsrc','edw_01008_N03','edpf_rsrc_N03'),
                ('edpf_rsrc','edw_01008_N02','edpf_rsrc_N02'),
                ('edpf_rsrc','edw_01008_N01','edpf_rsrc_N01'),
                ('edpf_prj','edw_01007_N03','edpf_prj_N03'),
                ('edpf_prj','edw_01007_N02','edpf_prj_N02'),
                ('edpf_prj','edw_01007_N01','edpf_prj_N01'),
                ('edpf_prjmbr','edw_01006_N02','edpf_prjmbr_N02'),
                ('edpf_prjmbr','edw_01006_N01','edpf_prjmbr_N01'),
                ('edpf_orgmbr','edw_01005_N02','edpf_orgmbr_N02'),
                ('edpf_orgmbr','edw_01005_N01','edpf_orgmbr_N01'),
                ('edpf_usr','edw_01004_N01','edpf_usr_N01'),
                ('edpf_org','edw_01003_N02','edpf_org_N02'),
                ('edpf_org','edw_01003_N01','edpf_org_N01'),
                ('edpf_tntmbr','edw_01002_N01','edpf_tntmbr_N01'),
                ('edpf_tnt','edw_01001_N01','edpf_tnt_N01')
        ) AS v(table_name, source_name, target_name)
    LOOP
        source_table := CASE item.table_name
            WHEN 'edw_01001' THEN 'edpf_tnt'
            WHEN 'edw_01002' THEN 'edpf_tntmbr'
            WHEN 'edw_01003' THEN 'edpf_org'
            WHEN 'edw_01004' THEN 'edpf_usr'
            WHEN 'edw_01005' THEN 'edpf_orgmbr'
            WHEN 'edw_01006' THEN 'edpf_prjmbr'
            WHEN 'edw_01007' THEN 'edpf_prj'
            WHEN 'edw_01008' THEN 'edpf_rsrc'
            WHEN 'edw_01009' THEN 'edpf_plcy'
            WHEN 'edw_01010' THEN 'edpf_authplcy'
            WHEN 'edw_01011' THEN 'edpf_authaud'
            WHEN 'edw_01012' THEN 'edpf_authplcy_chg'
            WHEN 'edw_01013' THEN 'edpf_authaud_rtn'
            WHEN 'edw_01014' THEN 'edpf_authaud_rtnchg'
            WHEN 'edw_02001' THEN 'edpf_strqta_plcy'
            WHEN 'edw_02002' THEN 'edpf_strqta_plcychg'
            WHEN 'edw_03001' THEN 'edpf_authaud_arcjob'
            WHEN 'edw_03002' THEN 'edpf_authaud_arcattm'
            WHEN 'edw_03003' THEN 'edpf_authaud_arcschd'
            WHEN 'edw_04001' THEN 'edpf_tntdbnd'
            WHEN 'edw_04002' THEN 'edpf_tntrtnalw'
            WHEN 'edw_04003' THEN 'edpf_tntmut_aud'
            ELSE item.table_name
        END;
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = item.source_name)
           AND NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = item.target_name) THEN
            SELECT i.relname INTO existing_name
            FROM pg_index x
            JOIN pg_class i ON i.oid = x.indexrelid
            JOIN pg_class t ON t.oid = x.indrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'public'
              AND t.relname = source_table
              AND NOT x.indisprimary
            ORDER BY i.relname
            LIMIT 1;
            IF existing_name IS NOT NULL THEN
                EXECUTE format('ALTER INDEX %I RENAME TO %I', existing_name, item.source_name);
            END IF;
        ELSIF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = item.source_name)
              AND EXISTS (SELECT 1 FROM pg_class WHERE relname = item.target_name) THEN
            EXECUTE format('ALTER INDEX %I RENAME TO %I', item.target_name, item.source_name);
        END IF;
    END LOOP;
END $$;
""");
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

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edpf_strqta_plcy DROP CONSTRAINT IF EXISTS \"AK_edpf_strqta_plcy_str_qta_plcyid_tntid\";");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_strqta_plcy_PK",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_rsrc_PK",
                table: "edpf_rsrc");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prjmbr_PK",
                table: "edpf_prjmbr");

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edpf_prj DROP CONSTRAINT IF EXISTS \"AK_edpf_prj_prjid_orgid_tntid\";");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_prj_PK",
                table: "edpf_prj");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_plcy_PK",
                table: "edpf_plcy");

            migrationBuilder.DropPrimaryKey(
                name: "edpf_orgmbr_PK",
                table: "edpf_orgmbr");

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edpf_org DROP CONSTRAINT IF EXISTS \"AK_edpf_org_orgid_tntid\";");

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

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edpf_authaud_arcjob DROP CONSTRAINT IF EXISTS \"AK_edpf_authaud_arcjob_auth_aud_arc_jobid_tntid\";");

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

            migrationBuilder.AddForeignKey(
                name: "FK_platform_invitations_platform_tenants_TenantId",
                table: "edpf_inv",
                column: "TenantId",
                principalTable: "edw_01001",
                principalColumn: "01001id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_invitations_platform_users_UserId",
                table: "edpf_inv",
                column: "UserId",
                principalTable: "edw_01004",
                principalColumn: "01004id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edw_01002_FKTenantId",
                table: "edw_01002");

            migrationBuilder.DropForeignKey(
                name: "edw_01003_FKTenantId",
                table: "edw_01003");

            migrationBuilder.DropForeignKey(
                name: "edw_01004_FKTenantId",
                table: "edw_01004");

            migrationBuilder.DropForeignKey(
                name: "edw_01005_FKOrganizationId",
                table: "edw_01005");

            migrationBuilder.DropForeignKey(
                name: "edw_01006_FKProjectId",
                table: "edw_01006");

            migrationBuilder.DropForeignKey(
                name: "edw_01007_FKOrganizationId",
                table: "edw_01007");

            migrationBuilder.DropForeignKey(
                name: "edw_01007_FKTenantId",
                table: "edw_01007");

            migrationBuilder.DropForeignKey(
                name: "edw_01008_FKScopeOrganizationId",
                table: "edw_01008");

            migrationBuilder.DropForeignKey(
                name: "edw_01008_FKScopeProjectId",
                table: "edw_01008");

            migrationBuilder.DropForeignKey(
                name: "edw_01008_FKScopeTenantId",
                table: "edw_01008");

            migrationBuilder.DropForeignKey(
                name: "edw_01009_FKScopeOrganizationId",
                table: "edw_01009");

            migrationBuilder.DropForeignKey(
                name: "edw_01009_FKScopeProjectId",
                table: "edw_01009");

            migrationBuilder.DropForeignKey(
                name: "edw_01009_FKScopeTenantId",
                table: "edw_01009");

            migrationBuilder.DropForeignKey(
                name: "edw_01010_FKOrganizationId",
                table: "edw_01010");

            migrationBuilder.DropForeignKey(
                name: "edw_01010_FKProjectId",
                table: "edw_01010");

            migrationBuilder.DropForeignKey(
                name: "edw_01010_FKTenantId",
                table: "edw_01010");

            migrationBuilder.DropForeignKey(
                name: "edw_01011_FKTenantId",
                table: "edw_01011");

            migrationBuilder.DropForeignKey(
                name: "edw_01012_FKTenantId",
                table: "edw_01012");

            migrationBuilder.DropForeignKey(
                name: "edw_01013_FKTenantId",
                table: "edw_01013");

            migrationBuilder.DropForeignKey(
                name: "edw_01014_FKTenantId",
                table: "edw_01014");

            migrationBuilder.DropForeignKey(
                name: "edw_02001_FKTenantId",
                table: "edw_02001");

            migrationBuilder.DropForeignKey(
                name: "edw_02002_FKPolicyId",
                table: "edw_02002");

            migrationBuilder.DropForeignKey(
                name: "edw_02002_FKTenantId",
                table: "edw_02002");

            migrationBuilder.DropForeignKey(
                name: "edw_03001_FKTenantId",
                table: "edw_03001");

            migrationBuilder.DropForeignKey(
                name: "edw_03002_FKJobId",
                table: "edw_03002");

            migrationBuilder.DropForeignKey(
                name: "edw_03002_FKTenantId",
                table: "edw_03002");

            migrationBuilder.DropForeignKey(
                name: "edw_03003_FKTenantId",
                table: "edw_03003");

            migrationBuilder.DropForeignKey(
                name: "edw_04001_FKTenantId",
                table: "edw_04001");

            migrationBuilder.DropForeignKey(
                name: "edw_04002_FKTenantId",
                table: "edw_04002");

            migrationBuilder.DropForeignKey(
                name: "edw_04003_FKTenantId",
                table: "edw_04003");

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

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edw_03001 DROP CONSTRAINT IF EXISTS \"AK_edw_03001_03001id_tntid\";");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03001_PK",
                table: "edw_03001");

            migrationBuilder.DropPrimaryKey(
                name: "edw_02002_PK",
                table: "edw_02002");

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edw_02001 DROP CONSTRAINT IF EXISTS \"AK_edw_02001_02001id_tntid\";");

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

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edw_01007 DROP CONSTRAINT IF EXISTS \"AK_edw_01007_01007id_orgid_tntid\";");

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

            migrationBuilder.Sql("ALTER TABLE IF EXISTS edw_01003 DROP CONSTRAINT IF EXISTS \"AK_edw_01003_01003id_tntid\";");

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

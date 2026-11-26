using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class _AuditSequentialMappingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql +
    N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' +
    QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.key_constraints
WHERE type = 'UQ'
  AND name IN
  (N'AK_edw_03001_auth_aud_arc_jobid_tntid',
   N'AK_edw_02001_str_qta_plcyid_tntid',
   N'AK_edw_01007_prjid_orgid_tntid',
   N'AK_edw_01003_orgid_tntid');
IF @sql <> N'' EXEC sys.sp_executesql @sql;");

            // The preceding framework migration may already have applied the
            // sequential column names. Rewind only those columns so the
            // generated EF rename operations below remain idempotent.
            migrationBuilder.Sql(@"
DECLARE @table sysname, @old sysname, @new sysname, @sql nvarchar(max);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
SELECT table_name, old_name, new_name
FROM (VALUES
 (N'edw_04003',N'tnt_mut_audid',N'04003id'),
 (N'edw_04002',N'tnt_rtn_alwid',N'04002id'),
 (N'edw_04001',N'tnt_dbndid',N'04001id'),
 (N'edw_03003',N'auth_aud_arc_schdid',N'03003id'),
 (N'edw_03002',N'auth_aud_arc_attmid',N'03002id'),
 (N'edw_03001',N'auth_aud_arc_jobid',N'03001id'),
 (N'edw_02002',N'str_qta_plcy_chgid',N'02002id'),
 (N'edw_02001',N'str_qta_plcyid',N'02001id'),
 (N'edw_01014',N'auth_aud_rtn_chgid',N'01014id'),
 (N'edw_01013',N'auth_aud_rtnid',N'01013id'),
 (N'edw_01012',N'auth_plcy_chgid',N'01012id'),
 (N'edw_01011',N'auth_audid',N'01011id'),
 (N'edw_01010',N'auth_plcyid',N'01010id'),
 (N'edw_01009',N'plcyid',N'01009id')) AS m(table_name,old_name,new_name);
OPEN c;
FETCH NEXT FROM c INTO @table,@old,@new;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @table) IS NOT NULL
       AND COL_LENGTH(N'dbo.' + @table, @old) IS NULL
       AND COL_LENGTH(N'dbo.' + @table, @new) IS NOT NULL
    BEGIN
        SET @sql = N'EXEC sp_rename N''dbo.' + @table + N'.' + @new + N''', N''' + @old + N''', N''COLUMN'';';
        EXEC sp_executesql @sql;
    END;
    FETCH NEXT FROM c INTO @table,@old,@new;
END;
CLOSE c; DEALLOCATE c;");

            migrationBuilder.RenameColumn(
                name: "tnt_mut_audid",
                table: "edw_04003",
                newName: "04003id");

            migrationBuilder.RenameColumn(
                name: "tnt_rtn_alwid",
                table: "edw_04002",
                newName: "04002id");

            migrationBuilder.RenameColumn(
                name: "tnt_dbndid",
                table: "edw_04001",
                newName: "04001id");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_schdid",
                table: "edw_03003",
                newName: "03003id");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_attmid",
                table: "edw_03002",
                newName: "03002id");

            migrationBuilder.RenameColumn(
                name: "auth_aud_arc_jobid",
                table: "edw_03001",
                newName: "03001id");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcy_chgid",
                table: "edw_02002",
                newName: "02002id");

            migrationBuilder.RenameColumn(
                name: "str_qta_plcyid",
                table: "edw_02001",
                newName: "02001id");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtn_chgid",
                table: "edw_01014",
                newName: "01014id");

            migrationBuilder.RenameColumn(
                name: "auth_aud_rtnid",
                table: "edw_01013",
                newName: "01013id");

            migrationBuilder.RenameColumn(
                name: "auth_plcy_chgid",
                table: "edw_01012",
                newName: "01012id");

            migrationBuilder.RenameColumn(
                name: "auth_audid",
                table: "edw_01011",
                newName: "01011id");

            migrationBuilder.RenameColumn(
                name: "auth_plcyid",
                table: "edw_01010",
                newName: "01010id");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edw_01009",
                newName: "01009id");

            migrationBuilder.RenameColumn(
                name: "rsrcnm",
                table: "edw_01008",
                newName: "01008nm");

            migrationBuilder.RenameColumn(
                name: "rsrcid",
                table: "edw_01008",
                newName: "01008id");

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

            migrationBuilder.RenameColumn(
                name: "prj_mbrid",
                table: "edw_01006",
                newName: "01006id");

            migrationBuilder.RenameColumn(
                name: "org_mbrid",
                table: "edw_01005",
                newName: "01005id");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edw_01004",
                newName: "01004id");

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

            migrationBuilder.RenameColumn(
                name: "tnt_mbrid",
                table: "edw_01002",
                newName: "01002id");

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

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_03001_03001id_tntid",
                table: "edw_03001",
                columns: new[] { "03001id", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_02001_02001id_tntid",
                table: "edw_02001",
                columns: new[] { "02001id", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01007_01007id_orgid_tntid",
                table: "edw_01007",
                columns: new[] { "01007id", "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01003_01003id_tntid",
                table: "edw_01003",
                columns: new[] { "01003id", "tntid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_03001_03001id_tntid",
                table: "edw_03001");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_02001_02001id_tntid",
                table: "edw_02001");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_01007_01007id_orgid_tntid",
                table: "edw_01007");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edw_01003_01003id_tntid",
                table: "edw_01003");

            migrationBuilder.RenameColumn(
                name: "04003id",
                table: "edw_04003",
                newName: "tnt_mut_audid");

            migrationBuilder.RenameColumn(
                name: "04002id",
                table: "edw_04002",
                newName: "tnt_rtn_alwid");

            migrationBuilder.RenameColumn(
                name: "04001id",
                table: "edw_04001",
                newName: "tnt_dbndid");

            migrationBuilder.RenameColumn(
                name: "03003id",
                table: "edw_03003",
                newName: "auth_aud_arc_schdid");

            migrationBuilder.RenameColumn(
                name: "03002id",
                table: "edw_03002",
                newName: "auth_aud_arc_attmid");

            migrationBuilder.RenameColumn(
                name: "03001id",
                table: "edw_03001",
                newName: "auth_aud_arc_jobid");

            migrationBuilder.RenameColumn(
                name: "02002id",
                table: "edw_02002",
                newName: "str_qta_plcy_chgid");

            migrationBuilder.RenameColumn(
                name: "02001id",
                table: "edw_02001",
                newName: "str_qta_plcyid");

            migrationBuilder.RenameColumn(
                name: "01014id",
                table: "edw_01014",
                newName: "auth_aud_rtn_chgid");

            migrationBuilder.RenameColumn(
                name: "01013id",
                table: "edw_01013",
                newName: "auth_aud_rtnid");

            migrationBuilder.RenameColumn(
                name: "01012id",
                table: "edw_01012",
                newName: "auth_plcy_chgid");

            migrationBuilder.RenameColumn(
                name: "01011id",
                table: "edw_01011",
                newName: "auth_audid");

            migrationBuilder.RenameColumn(
                name: "01010id",
                table: "edw_01010",
                newName: "auth_plcyid");

            migrationBuilder.RenameColumn(
                name: "01009id",
                table: "edw_01009",
                newName: "plcyid");

            migrationBuilder.RenameColumn(
                name: "01008nm",
                table: "edw_01008",
                newName: "rsrcnm");

            migrationBuilder.RenameColumn(
                name: "01008id",
                table: "edw_01008",
                newName: "rsrcid");

            migrationBuilder.RenameColumn(
                name: "01007nm",
                table: "edw_01007",
                newName: "prjnm");

            migrationBuilder.RenameColumn(
                name: "01007cd",
                table: "edw_01007",
                newName: "prjcd");

            migrationBuilder.RenameColumn(
                name: "01007id",
                table: "edw_01007",
                newName: "prjid");

            migrationBuilder.RenameColumn(
                name: "01006id",
                table: "edw_01006",
                newName: "prj_mbrid");

            migrationBuilder.RenameColumn(
                name: "01005id",
                table: "edw_01005",
                newName: "org_mbrid");

            migrationBuilder.RenameColumn(
                name: "01004id",
                table: "edw_01004",
                newName: "usrid");

            migrationBuilder.RenameColumn(
                name: "01003nm",
                table: "edw_01003",
                newName: "orgnm");

            migrationBuilder.RenameColumn(
                name: "01003cd",
                table: "edw_01003",
                newName: "orgcd");

            migrationBuilder.RenameColumn(
                name: "01003id",
                table: "edw_01003",
                newName: "orgid");

            migrationBuilder.RenameColumn(
                name: "01002id",
                table: "edw_01002",
                newName: "tnt_mbrid");

            migrationBuilder.RenameColumn(
                name: "01001nm",
                table: "edw_01001",
                newName: "tntnm");

            migrationBuilder.RenameColumn(
                name: "01001cd",
                table: "edw_01001",
                newName: "tntcd");

            migrationBuilder.RenameColumn(
                name: "01001id",
                table: "edw_01001",
                newName: "tntid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_03001_auth_aud_arc_jobid_tntid",
                table: "edw_03001",
                columns: new[] { "auth_aud_arc_jobid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_02001_str_qta_plcyid_tntid",
                table: "edw_02001",
                columns: new[] { "str_qta_plcyid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01007_prjid_orgid_tntid",
                table: "edw_01007",
                columns: new[] { "prjid", "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edw_01003_orgid_tntid",
                table: "edw_01003",
                columns: new[] { "orgid", "tntid" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class _AuditSequentialMappingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The preceding DataSet table migration may already have applied
            // the sequential names. Rewind only those columns so the generated
            // EF renames below are safe to replay.
            migrationBuilder.Sql(@"
DECLARE @table sysname, @old sysname, @new sysname, @sql nvarchar(max);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
SELECT table_name, old_name, new_name
FROM (VALUES
 (N'edw_05009',N'dset_vld_seen_keyid',N'05009id'),
 (N'edw_05008',N'dset_vld_dtlcd',N'05008cd'),
 (N'edw_05008',N'dset_vld_dtlid',N'05008id'),
 (N'edw_05007',N'dset_vld_excid',N'05007id'),
 (N'edw_05005',N'dset_vld_prfnm',N'05005nm'),
 (N'edw_05005',N'dset_vld_prfid',N'05005id'),
 (N'edw_05004',N'dset_qta_thr_evtid',N'05004id'),
 (N'edw_05003',N'dset_str_usg_snpid',N'05003id'),
 (N'edw_05001',N'dsetnm',N'05001nm'),
 (N'edw_05001',N'dsetid',N'05001id')) AS m(table_name,old_name,new_name);
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
                name: "dset_vld_seen_keyid",
                table: "edw_05009",
                newName: "05009id");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlcd",
                table: "edw_05008",
                newName: "05008cd");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlid",
                table: "edw_05008",
                newName: "05008id");

            migrationBuilder.RenameColumn(
                name: "dset_vld_excid",
                table: "edw_05007",
                newName: "05007id");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfnm",
                table: "edw_05005",
                newName: "05005nm");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfid",
                table: "edw_05005",
                newName: "05005id");

            migrationBuilder.RenameColumn(
                name: "dset_qta_thr_evtid",
                table: "edw_05004",
                newName: "05004id");

            migrationBuilder.RenameColumn(
                name: "dset_str_usg_snpid",
                table: "edw_05003",
                newName: "05003id");

            migrationBuilder.RenameColumn(
                name: "dsetnm",
                table: "edw_05001",
                newName: "05001nm");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "edw_05001",
                newName: "05001id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "05009id",
                table: "edw_05009",
                newName: "dset_vld_seen_keyid");

            migrationBuilder.RenameColumn(
                name: "05008cd",
                table: "edw_05008",
                newName: "dset_vld_dtlcd");

            migrationBuilder.RenameColumn(
                name: "05008id",
                table: "edw_05008",
                newName: "dset_vld_dtlid");

            migrationBuilder.RenameColumn(
                name: "05007id",
                table: "edw_05007",
                newName: "dset_vld_excid");

            migrationBuilder.RenameColumn(
                name: "05005nm",
                table: "edw_05005",
                newName: "dset_vld_prfnm");

            migrationBuilder.RenameColumn(
                name: "05005id",
                table: "edw_05005",
                newName: "dset_vld_prfid");

            migrationBuilder.RenameColumn(
                name: "05004id",
                table: "edw_05004",
                newName: "dset_qta_thr_evtid");

            migrationBuilder.RenameColumn(
                name: "05003id",
                table: "edw_05003",
                newName: "dset_str_usg_snpid");

            migrationBuilder.RenameColumn(
                name: "05001nm",
                table: "edw_05001",
                newName: "dsetnm");

            migrationBuilder.RenameColumn(
                name: "05001id",
                table: "edw_05001",
                newName: "dsetid");
        }
    }
}
